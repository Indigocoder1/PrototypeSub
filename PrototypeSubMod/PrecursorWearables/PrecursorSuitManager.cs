using PrototypeSubMod.Prefabs;
using SuitLib;
using UnityEngine;

namespace PrototypeSubMod.PrecursorWearables;

public class PrecursorSuitManager : MonoBehaviour
{
    private const float SuitEmission = 1f;
    
    private Renderer stillsuitRenderer;
    private float[] originalBodyEmissions = new float[2];
    private float[] originalArmsEmission = new float[2];
    private bool wasArmEmissionEnabled;
    private bool wasWearingSuit;
    
    private void Start()
    {
        ModdedSuitsManager.onSuitEquippedChanged += OnEquippedSuitChanged;
        stillsuitRenderer = transform.Find("body/player_view/male_geo/stillSuit/still_suit_01_body_geo")
            .GetComponent<Renderer>();
    }

    private void OnEquippedSuitChanged(TechType suitChanged)
    {
        var itemInSlot = Inventory.main.equipment.GetItemInSlot("Body");
        bool wearingSuit = itemInSlot != null && itemInSlot.techType == PrecursorSuit.PrefabInfo.TechType;
        if (wearingSuit != wasWearingSuit)
        {
            if (!wasWearingSuit)
            {
                StoreProperties();
                stillsuitRenderer.materials[0].SetFloat("_GlowStrength", SuitEmission);
                stillsuitRenderer.materials[0].SetFloat("_GlowStrengthNight", SuitEmission);
                
                stillsuitRenderer.materials[1].SetFloat("_GlowStrength", SuitEmission);
                stillsuitRenderer.materials[1].SetFloat("_GlowStrengthNight", SuitEmission);
                stillsuitRenderer.materials[1].EnableKeyword("MARMO_EMISSION");
            }
            else
            {
                RestoreProperties();
            }
        }
        
        wasWearingSuit = wearingSuit;
    }

    private void StoreProperties()
    {
        originalBodyEmissions[0] = stillsuitRenderer.materials[0].GetFloat("_GlowStrength");
        originalBodyEmissions[1] = stillsuitRenderer.materials[0].GetFloat("_GlowStrengthNight");
                
        originalArmsEmission[0] = stillsuitRenderer.materials[1].GetFloat("_GlowStrength");
        originalArmsEmission[1] = stillsuitRenderer.materials[1].GetFloat("_GlowStrengthNight");
        wasArmEmissionEnabled = stillsuitRenderer.materials[1].IsKeywordEnabled("MARMO_EMISSION");
    }

    private void RestoreProperties()
    {
        stillsuitRenderer.materials[0].SetFloat("_GlowStrength", originalBodyEmissions[0]);
        stillsuitRenderer.materials[0].SetFloat("_GlowStrengthNight", originalBodyEmissions[1]);
                
        stillsuitRenderer.materials[1].SetFloat("_GlowStrength", originalArmsEmission[0]);
        stillsuitRenderer.materials[1].SetFloat("_GlowStrengthNight", originalArmsEmission[1]);
        if (wasArmEmissionEnabled)
        {
            stillsuitRenderer.materials[1].EnableKeyword("MARMO_EMISSION");
        }
        else
        {
            stillsuitRenderer.materials[1].DisableKeyword("MARMO_EMISSION");
        }
    }

    private void OnDestroy()
    {
        ModdedSuitsManager.onSuitEquippedChanged -= OnEquippedSuitChanged;
    }
}