using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Utility;
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
    private bool hadSuit;

    private void Start()
    {
        for (int i = 1; i <= 4; i++)
        {
            factorSlots.Add(transform.Find($"Equipment/ProtoFactorSlot{i}").GetComponent<uGUI_EquipmentSlot>());
        }
        
        Inventory.main.equipment.onEquip += OnEquip;
    }

    // Called via SendMessage
    public void RefreshFactorSlots()
    {
        UWE.CoroutineHost.StartCoroutine(RefreshSlotsDelayed());
    }

    // Delayed to not cause issues for things like hotswapping suits
    private IEnumerator RefreshSlotsDelayed()
    {
        yield return null;
        
        var hasSuit = Inventory.main.equipment.GetTechTypeInSlot("Body") == PrecursorSuit.prefabInfo.TechType;

        if (hasSuit == hadSuit) yield break;
        
        if (hasSuit)
        {
            Inventory.main.equipment.AddSlots(FactorSlots);
        }
        else
        {
            foreach (var slot in FactorSlots)
            {
                Inventory.main.equipment.RemoveItem(slot, true, true);
                Inventory.main.equipment.RemoveSlot(slot);
            }
        }
            
        foreach (var slot in factorSlots)
        {
            slot.SetActive(hasSuit);
        }

        hadSuit = hasSuit;
    }

    private void OnEquip(string slot, InventoryItem item)
    {
        if (WaitScreen.IsWaiting) return;
        
        if (Array.IndexOf(FactorSlots, slot) < 0)
        {
            return;
        }

        FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("FactorEquip"), Player.main.transform.position);

    }

    private void OnDestroy()
    {
        Inventory.main.equipment.onEquip -= OnEquip;
    }
}