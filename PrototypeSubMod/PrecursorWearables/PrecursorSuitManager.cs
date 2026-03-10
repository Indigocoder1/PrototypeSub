using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrototypeSubMod.Patches;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.Prefabs.AlienBuildingBlock;
using SuitLib;
using UnityEngine;

namespace PrototypeSubMod.PrecursorWearables;

public class PrecursorSuitManager : MonoBehaviour
{
    private readonly float defaultEmissionIntensity = 0.1f;
    private readonly Color defaultEmissionColor = Color.white;
    private readonly float[] originalBodyEmissions = new float[2];
    private readonly float[] originalArmsEmission = new float[2];
    private readonly Color[] originalEmissionCols = new Color[2];

    private const float TimeBetweenWarperRemnants = 120f;
    
    private Dictionary<Component, EmissionController> emissionControllers = new();
    private Renderer stillsuitRenderer;
    private Color emissionColor;
    private float emissionIntensity;
    private bool wasArmEmissionEnabled;
    private bool wasWearingSuit;
    
    private void Start()
    {
        ModdedSuitsManager.onSuitEquippedChanged += OnEquippedSuitChanged;
        stillsuitRenderer = transform.Find("body/player_view/male_geo/stillSuit/still_suit_01_body_geo")
            .GetComponent<Renderer>();
        emissionColor = defaultEmissionColor;
        emissionIntensity = defaultEmissionIntensity;

        TooltipFactory_Patches.onRunItemActions += UpdateFromUI;
    }

    private void Update()
    {
        if (!wasWearingSuit) return;

        UpdateEmissionValues();
    }

    private void OnEquippedSuitChanged(TechType suitChanged)
    {
        var itemInSlot = Inventory.main.equipment.GetItemInSlot("Body");
        bool wearingSuit = itemInSlot != null && itemInSlot.techType == PrecursorSuit.prefabInfo.TechType;
        if (wearingSuit != wasWearingSuit)
        {
            if (wearingSuit)
            {
                StoreProperties();
                UpdateEmissionValues();
                stillsuitRenderer.materials[1].EnableKeyword("MARMO_EMISSION");
                InvokeRepeating(nameof(GivePlayerWarperRemnant), TimeBetweenWarperRemnants, TimeBetweenWarperRemnants);
            }
            else
            {
                RestoreProperties();
                CancelInvoke(nameof(GivePlayerWarperRemnant));
            }
        }
        
        wasWearingSuit = wearingSuit;
    }

    private void UpdateEmissionValues()
    {
        stillsuitRenderer.materials[0].SetFloat(ShaderPropertyID._GlowStrength, emissionIntensity);
        stillsuitRenderer.materials[0].SetFloat(ShaderPropertyID._GlowStrengthNight, emissionIntensity);
        stillsuitRenderer.materials[0].SetColor(ShaderPropertyID._GlowColor, emissionColor);
                
        stillsuitRenderer.materials[1].SetFloat(ShaderPropertyID._GlowStrength, emissionIntensity);
        stillsuitRenderer.materials[1].SetFloat(ShaderPropertyID._GlowStrengthNight, emissionIntensity);
        stillsuitRenderer.materials[1].SetColor(ShaderPropertyID._GlowColor, emissionColor);
    }

    private void StoreProperties()
    {
        originalBodyEmissions[0] = stillsuitRenderer.materials[0].GetFloat(ShaderPropertyID._GlowStrength);
        originalBodyEmissions[1] = stillsuitRenderer.materials[0].GetFloat(ShaderPropertyID._GlowStrengthNight);
                
        originalArmsEmission[0] = stillsuitRenderer.materials[1].GetFloat(ShaderPropertyID._GlowStrength);
        originalArmsEmission[1] = stillsuitRenderer.materials[1].GetFloat(ShaderPropertyID._GlowStrengthNight);
        wasArmEmissionEnabled = stillsuitRenderer.materials[1].IsKeywordEnabled("MARMO_EMISSION");

        originalEmissionCols[0] = stillsuitRenderer.materials[0].GetColor(ShaderPropertyID._GlowColor);
        originalEmissionCols[1] = stillsuitRenderer.materials[1].GetColor(ShaderPropertyID._GlowColor);
    }

    private void RestoreProperties()
    {
        stillsuitRenderer.materials[0].SetFloat(ShaderPropertyID._GlowStrength, originalBodyEmissions[0]);
        stillsuitRenderer.materials[0].SetFloat(ShaderPropertyID._GlowStrengthNight, originalBodyEmissions[1]);
                
        stillsuitRenderer.materials[1].SetFloat(ShaderPropertyID._GlowStrength, originalArmsEmission[0]);
        stillsuitRenderer.materials[1].SetFloat(ShaderPropertyID._GlowStrengthNight, originalArmsEmission[1]);
        
        stillsuitRenderer.materials[0].SetColor(ShaderPropertyID._GlowColor, originalEmissionCols[0]);
        stillsuitRenderer.materials[1].SetColor(ShaderPropertyID._GlowColor, originalEmissionCols[1]);
        if (wasArmEmissionEnabled)
        {
            stillsuitRenderer.materials[1].EnableKeyword("MARMO_EMISSION");
        }
        else
        {
            stillsuitRenderer.materials[1].DisableKeyword("MARMO_EMISSION");
        }
    }

    private void GivePlayerWarperRemnant()
    {
        if (!Plugin.GlobalSaveData.precursorSuitGivesRemnants) return;

        StartCoroutine(SpawnRemnantAsync());
    }

    private IEnumerator SpawnRemnantAsync()
    {
        var task = CraftData.GetPrefabForTechTypeAsync(WarperRemnant.prefabInfo.TechType);
        yield return task;

        var prefab = task.GetResult();
        var pickupable = GameObject.Instantiate(prefab).GetComponent<Pickupable>();
        Inventory.main.ForcePickup(pickupable);
    }

    private void UpdateFromUI()
    {
        if (IngameMenu.main.selected) return;

        if (!GameInput.GetButtonDown(GameInput.Button.AltTool)) return;

        Plugin.GlobalSaveData.precursorSuitGivesRemnants = !Plugin.GlobalSaveData.precursorSuitGivesRemnants;
    }

    private void OnDestroy()
    {
        ModdedSuitsManager.onSuitEquippedChanged -= OnEquippedSuitChanged;
        TooltipFactory_Patches.onRunItemActions -= UpdateFromUI;
    }

    public void RegisterEmissionController(Component owner, EmissionController controller)
    {
        emissionControllers[owner] = controller;
        var highestPriorityItem = emissionControllers.OrderByDescending(kvp => kvp.Value.priority).ElementAt(0).Value;
        emissionIntensity = highestPriorityItem.emissionIntensity;
        emissionColor = highestPriorityItem.emissionColor;
    }

    public void DeregisterEmissionController(Component owner)
    {
        emissionControllers.Remove(owner);
        if (emissionControllers.Count == 0)
        {
            emissionIntensity = defaultEmissionIntensity;
            emissionColor = defaultEmissionColor;
        }
        else
        {
            var highestPriorityItem = emissionControllers.OrderByDescending(kvp => kvp.Value.priority).ElementAt(0).Value;
            emissionIntensity = highestPriorityItem.emissionIntensity;
            emissionColor = highestPriorityItem.emissionColor;
        }
    }
    
    public struct EmissionController
    {
        public Color emissionColor;
        public float emissionIntensity;
        public int priority;

        public EmissionController(Color emissionColor, float emissionIntensity, int priority = 10)
        {
            this.emissionColor = emissionColor;
            this.emissionIntensity = emissionIntensity;
            this.priority = priority;
        }
    }
}