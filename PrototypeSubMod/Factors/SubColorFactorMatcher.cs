using System;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using UnityEngine;

namespace PrototypeSubMod.Factors;

public class SubColorFactorMatcher : MonoBehaviour
{
    [SerializeField] private EmissionColorController emissionColorController;

    private void OnEnable()
    {
        ColorFactor.OnChangeSubEmission += UpdateFromSuit;
        FactorActivationManager.onEquippedFactor += OnEquipFactor;
        FactorActivationManager.onUnequippedFactor += OnUnequipFactor;
    }

    private void OnDisable()
    {
        ColorFactor.OnChangeSubEmission -= UpdateFromSuit;
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

        var modifiedColor = colorFactor.GetCurrentSubColor() * colorFactor.GetSubIntensity();
        emissionColorController.RegisterTempColor(this, new EmissionColorController.EmissionRegistrarData(modifiedColor));
    }
    
    private void OnUnequipFactor(Factor factor)
    {
        if (factor is not ColorFactor) return;
        
        emissionColorController.RemoveTempColor(this);
    }
}