using System;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using UnityEngine;

namespace PrototypeSubMod.Factors;

public class SubColorFactorMatcher : MonoBehaviour
{
    [SerializeField] private EmissionColorController emissionColorController;

    private void OnEnable()
    {
        ColorFactor.OnChangeSuitEmission += UpdateFromSuit;
        FactorActivationManager.onEquippedFactor += OnEquipFactor;
        FactorActivationManager.onUnequippedFactor += OnUnequipFactor;
    }

    private void OnDisable()
    {
        ColorFactor.OnChangeSuitEmission -= UpdateFromSuit;
        FactorActivationManager.onEquippedFactor -= OnEquipFactor;
        FactorActivationManager.onUnequippedFactor -= OnUnequipFactor;
    }

    private void UpdateFromSuit(Color color, float intensity)
    {
        var modifiedColor = color * intensity;
        emissionColorController.RegisterTempColor(this, new EmissionColorController.EmissionRegistrarData(modifiedColor));
    }

    private void OnEquipFactor(Factor factor)
    {
        if (factor is not ColorFactor colorFactor) return;

        var modifiedColor = colorFactor.GetCurrentColor() * colorFactor.GetIntensity();
        emissionColorController.RegisterTempColor(this, new EmissionColorController.EmissionRegistrarData(modifiedColor));
    }
    
    private void OnUnequipFactor(Factor factor)
    {
        if (factor is not ColorFactor) return;
        
        emissionColorController.RemoveTempColor(this);
    }
}