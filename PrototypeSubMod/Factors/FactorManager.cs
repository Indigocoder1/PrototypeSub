using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrototypeSubMod.Factors;

public class FactorManager : MonoBehaviour
{
    private readonly List<Factor> equippedFactors = new();
    private readonly Dictionary<Factor, float> nextUseTime = new();
    
    private void Start()
    {
        Inventory.main.equipment.onEquip += RegisterEquipped;
        Inventory.main.equipment.onUnequip += RegisterUnequipped;
    }

    private void RegisterEquipped(string slot, InventoryItem item)
    {
        if (item.item == null) return;
        
        var factorComponents = item.item.GetComponents(typeof(Factor));
        if (factorComponents.Length == 0) return;

        foreach (var factor in factorComponents)
        {
            TryRegisterFactor(factor as Factor);
        }
    }

    private void TryRegisterFactor(Factor factor)
    {
        if (equippedFactors.Contains(factor)) return;
        
        factor.OnEquipped();
        equippedFactors.Add(factor);
        
        if(!nextUseTime.ContainsKey(factor))
            nextUseTime.Add(factor, Time.time);
    }

    private void RegisterUnequipped(string slot, InventoryItem item)
    {
        if (item.item == null) return;
        
        var factorComponents = item.item.GetComponents(typeof(Factor));
        if (factorComponents.Length == 0) return;

        foreach (var factor in factorComponents)
        {
            DeregisterFactor(factor as Factor);
        }
    }

    private void DeregisterFactor(Factor factor)
    {
        factor.OnUnequipped();
        equippedFactors.Remove(factor);
    }
    
    public void Update()
    {
        if (equippedFactors.Count == 0) return;
        
        if (IngameMenu.main.gameObject.activeSelf) return;
        
        for (int i = 0; i < equippedFactors.Count; i++)
        {
            var factor = equippedFactors[i];

            factor.UpdateFactor();
            
            if (!GameInput.GetButtonHeld(factor.GetUseButton()))
            {
                if (factor.InUse()) factor.StopUse();
                continue;
            }
            
            if (Time.time >= nextUseTime[factor] && GameInput.GetButtonDown(factor.GetUseButton()))
            {
                nextUseTime[factor] = Time.time + factor.cooldown;
                factor.StartUse();
            }
        }
    }
    
    public bool ContainsFactor(Factor factor)
    {
        return equippedFactors.Contains(factor);
    }
}