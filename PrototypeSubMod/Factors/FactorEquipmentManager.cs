using System;
using System.Collections.Generic;
using System.Linq;
using PrototypeSubMod.Prefabs;
using UnityEngine;

namespace PrototypeSubMod.Factors;

public class FactorEquipmentManager : MonoBehaviour
{
    public static string[] FactorSlots = {
        "ProtoFactorSlot1",
        "ProtoFactorSlot2",
        "ProtoFactorSlot3",
        "ProtoFactorSlot4"
    };

    private List<uGUI_EquipmentSlot> factorSlots = new();
    private uGUI_Equipment uGUIEquipment;
    private bool wasShowingSlots;

    private void Start()
    {
        for (int i = 1; i <= 4; i++)
        {
            factorSlots.Add(transform.Find($"Equipment/ProtoFactorSlot{i}").GetComponent<uGUI_EquipmentSlot>());
        }

        uGUIEquipment = GetComponentInChildren<uGUI_Equipment>(true);
    }

    private void RefreshFactorSlots()
    {
        if (!uGUIEquipment.gameObject.activeSelf) return;
        
        var hasSuit = Inventory.main.equipment.GetTechTypeInSlot("Body") == PrecursorSuit.PrefabInfo.TechType;
        bool showSlots = hasSuit && Inventory.main.usedStorage.Count == 0;

        if (showSlots != wasShowingSlots)
        {
            if (showSlots)
            {
                uGUIEquipment.equipment.AddSlots(FactorSlots);
            }
            else
            {
                foreach (var slot in FactorSlots)
                {
                    uGUIEquipment.equipment.equipment.Remove(slot);
                }
            }
            
            foreach (var slot in factorSlots)
            {
                slot.SetActive(showSlots);
            }
        }

        wasShowingSlots = showSlots;
    }
}