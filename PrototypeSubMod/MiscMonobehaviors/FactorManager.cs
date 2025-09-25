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
        
        equippedFactors.Add(factor.name, factor);
        
        if(!nextUseTime.ContainsKey(factor.name))
            nextUseTime.Add(factor.name, Time.time);
    }

    private void RegisterUnequipped(string slot, InventoryItem item)
    {
        if (item.item == null || !item.item.TryGetComponent(out Factor factor))
            return;
        
        equippedFactors.Remove(factor.name);
    }
    
    private void Update()
    {
        if (GameInput.GetButtonDown(GameInput.Button.AltTool) && equippedFactors.Count > 0)
        {
            for (int i = 0; i < equippedFactors.Count; i++)
            {
                var factor = equippedFactors.ElementAt(i).Value;

                if (Time.time >= nextUseTime[factor.name])
                {
                    nextUseTime[factor.name] = Time.time + factor.cooldown;
                    factor.Use();
                }
            }
        }
    }
    
    public bool ContainsFactor(string factorName)
    {
        return equippedFactors.ContainsKey(factorName);
    }
}