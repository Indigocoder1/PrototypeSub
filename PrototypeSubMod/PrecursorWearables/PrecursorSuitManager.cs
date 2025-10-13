using System;
using PrototypeSubMod.Prefabs;
using SuitLib;
using UnityEngine;

namespace PrototypeSubMod.PrecursorWearables;

public class PrecursorSuitManager : MonoBehaviour
{
    private float suitEmission = 0f;
    private Color emissionColor = Color.white;
    
    private Renderer stillsuitRenderer;
    private readonly float[] originalBodyEmissions = new float[2];
    private readonly float[] originalArmsEmission = new float[2];
    private readonly Color[] originalEmissionCols = new Color[2];
    private bool wasArmEmissionEnabled;
    private bool wasWearingSuit;
    
    private void Start()
    {
        ModdedSuitsManager.onSuitEquippedChanged += OnEquippedSuitChanged;
        stillsuitRenderer = transform.Find("body/player_view/male_geo/stillSuit/still_suit_01_body_geo")
            .GetComponent<Renderer>();
    }

    private void Update()
    {
        if (!wasWearingSuit) return;

        UpdateEmissionValues();
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
                UpdateEmissionValues();
                stillsuitRenderer.materials[1].EnableKeyword("MARMO_EMISSION");
            }
            else
            {
                RestoreProperties();
            }
        }
        
        wasWearingSuit = wearingSuit;
    }

    private void UpdateEmissionValues()
    {
        stillsuitRenderer.materials[0].SetFloat(ShaderPropertyID._GlowStrength, suitEmission);
        stillsuitRenderer.materials[0].SetFloat(ShaderPropertyID._GlowStrengthNight, suitEmission);
        stillsuitRenderer.materials[0].SetColor(ShaderPropertyID._EmissionColor, emissionColor);
                
        stillsuitRenderer.materials[1].SetFloat(ShaderPropertyID._GlowStrength, suitEmission);
        stillsuitRenderer.materials[1].SetFloat(ShaderPropertyID._GlowStrengthNight, suitEmission);
        stillsuitRenderer.materials[1].SetColor(ShaderPropertyID._EmissionColor, emissionColor);
    }

    private void StoreProperties()
    {
        originalBodyEmissions[0] = stillsuitRenderer.materials[0].GetFloat(ShaderPropertyID._GlowStrength);
        originalBodyEmissions[1] = stillsuitRenderer.materials[0].GetFloat(ShaderPropertyID._GlowStrengthNight);
                
        originalArmsEmission[0] = stillsuitRenderer.materials[1].GetFloat(ShaderPropertyID._GlowStrength);
        originalArmsEmission[1] = stillsuitRenderer.materials[1].GetFloat(ShaderPropertyID._GlowStrengthNight);
        wasArmEmissionEnabled = stillsuitRenderer.materials[1].IsKeywordEnabled("MARMO_EMISSION");

        originalEmissionCols[0] = stillsuitRenderer.materials[0].GetColor(ShaderPropertyID._EmissionColor);
        originalEmissionCols[1] = stillsuitRenderer.materials[1].GetColor(ShaderPropertyID._EmissionColor);
    }

    private void RestoreProperties()
    {
        stillsuitRenderer.materials[0].SetFloat(ShaderPropertyID._GlowStrength, originalBodyEmissions[0]);
        stillsuitRenderer.materials[0].SetFloat(ShaderPropertyID._GlowStrengthNight, originalBodyEmissions[1]);
                
        stillsuitRenderer.materials[1].SetFloat(ShaderPropertyID._GlowStrength, originalArmsEmission[0]);
        stillsuitRenderer.materials[1].SetFloat(ShaderPropertyID._GlowStrengthNight, originalArmsEmission[1]);
        
        stillsuitRenderer.materials[0].SetColor(ShaderPropertyID._EmissionColor, originalEmissionCols[0]);
        stillsuitRenderer.materials[1].SetColor(ShaderPropertyID._EmissionColor, originalEmissionCols[1]);
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