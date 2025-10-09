using System;
using System.Collections.Generic;
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

    private List<GameObject> factorSlots = new();

    private void Start()
    {
        for (int i = 1; i <= 4; i++)
        {
            factorSlots.Add(transform.Find($"Equipment/ProtoFactorSlot{i}").gameObject);
        }
    }

    private void RefreshFactorSlots()
    {
        var hasSuit = Inventory.main.equipment.GetTechTypeInSlot("Body") == PrecursorSuit.PrefabInfo.TechType;
        foreach (var slot in factorSlots)
        {
            slot.SetActive(hasSuit && Inventory.main.usedStorage.Count == 0);
        }
    }
}