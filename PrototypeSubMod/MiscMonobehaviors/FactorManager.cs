using System;
using System.Collections.Generic;
using System.Linq;
using PrototypeSubMod.Factors;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class FactorManager : MonoBehaviour
{
    private Dictionary<string, Factor> equippedFactors = new();
    private Dictionary<string, float> nextUseTime = new();
    
    private void Start()
    {
        Inventory.main.equipment.onEquip += RegisterEquipped;
        Inventory.main.equipment.onUnequip += RegisterUnequipped;
    }

    private void RegisterEquipped(string slot, InventoryItem item)
    {
        if (item.item == null || !item.item.TryGetComponent(out Factor factor))
            return;

        if (equippedFactors.ContainsValue(factor)) return;
        
        factor.OnEquipped();
        equippedFactors.Add(factor.name, factor);
        
        if(!nextUseTime.ContainsKey(factor.name))
            nextUseTime.Add(factor.name, Time.time);
    }

    private void RegisterUnequipped(string slot, InventoryItem item)
    {
        if (item.item == null || !item.item.TryGetComponent(out Factor factor))
            return;
        
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