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
    private uGUI_Equipment uGUIEquipment;
    private bool wasShowingSlots;

    private void Start()
    {
        for (int i = 1; i <= 4; i++)
        {
            factorSlots.Add(transform.Find($"Equipment/ProtoFactorSlot{i}").GetComponent<uGUI_EquipmentSlot>());
        }

        uGUIEquipment = GetComponentInChildren<uGUI_Equipment>(true);
        Inventory.main.equipment.onEquip += OnEquip;
    }

    // Called via SendMessage
    private void RefreshFactorSlots()
    {
        UWE.CoroutineHost.StartCoroutine(RefreshSlotsDelayed());
    }

    // Delayed to not cause issues for things like hotswapping suits
    private IEnumerator RefreshSlotsDelayed()
    {
        yield return null;
        
        if (!uGUIEquipment.gameObject.activeSelf) yield break;
        
        var hasSuit = Inventory.main.equipment.GetTechTypeInSlot("Body") == PrecursorSuit.prefabInfo.TechType;
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

    private void OnEquip(string slot, InventoryItem item)
    {
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