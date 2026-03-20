using System;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using UnityEngine;

namespace PrototypeSubMod.Factors;

public class SubColorFactorMatcher : MonoBehaviour
{
    [SerializeField] private EmissionColorController emissionColorController;

    private void Awake()
    {
        ColorFactor.OnChangeSubEmission += UpdateFromSuit;
        FactorActivationManager.onEquippedFactor += OnEquipFactor;
        FactorActivationManager.onUnequippedFactor += OnUnequipFactor;
        TryMatchFromEquipped();
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

    private void TryMatchFromEquipped()
    {
        foreach (var slot in FactorEquipmentManager.FactorSlots)
        {
            var itemInSlot = Inventory.main.equipment.GetItemInSlot(slot);

            var factor = itemInSlot?.item.GetComponent<Factor>();
            if (factor is not ColorFactor colorFactor) continue;

            UpdateFromSuit(colorFactor.GetCurrentSubColor(), colorFactor.GetSubIntensity());
            Plugin.Logger.LogInfo($"Updating sub emission color");
            break;
        }
    }
    
    private void OnDestroy()
    {
        ColorFactor.OnChangeSubEmission -= UpdateFromSuit;
        FactorActivationManager.onEquippedFactor -= OnEquipFactor;
        FactorActivationManager.onUnequippedFactor -= OnUnequipFactor;
    }
}