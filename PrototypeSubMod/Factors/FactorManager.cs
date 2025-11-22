using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrototypeSubMod.Factors;

public class FactorManager : MonoBehaviour
{
    private readonly Dictionary<string, Factor> equippedFactors = new();
    private readonly Dictionary<string, float> nextUseTime = new();
    
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
        if (equippedFactors.ContainsValue(factor)) return;
        
        factor.OnEquipped();
        equippedFactors.Add(factor.name, factor);
        
        if(!nextUseTime.ContainsKey(factor.name))
            nextUseTime.Add(factor.name, Time.time);
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
        equippedFactors.Remove(factor.name);
    }
    
    public void Update()
    {
        if (equippedFactors.Count == 0) return;
        
        for (int i = 0; i < equippedFactors.Count; i++)
        {
            var factor = equippedFactors.ElementAt(i).Value;

            factor.UpdateFactor();
            
            if (!GameInput.GetButtonHeld(factor.GetUseButton()))
            {
                if (factor.InUse()) factor.StopUse();
                continue;
            }
            
            if (Time.time >= nextUseTime[factor.name] && GameInput.GetButtonDown(factor.GetUseButton()))
            {
                nextUseTime[factor.name] = Time.time + factor.cooldown;
                factor.Use();
            }
        }
    }
    
    public bool ContainsFactor(string factorName)
    {
        return equippedFactors.ContainsKey(factorName);
    }
}