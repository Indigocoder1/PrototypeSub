using HarmonyLib;
using PrototypeSubMod.DeployablesTerminal;
using PrototypeSubMod.PowerSystem;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PrototypeSubMod.Factors;
using PrototypeSubMod.Utility;
using PrototypeSubMod.VehicleAccess;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(uGUI_Equipment))]
internal class uGUI_Equipment_Patches
{
    [SaveStateReference]
    private static DraggedItem LastDraggedItem;

    [HarmonyPatch(nameof(uGUI_Equipment.Awake)), HarmonyPrefix]
    private static void Awake_Prefix(uGUI_Equipment __instance)
    {
        CloneSlots(__instance, PrototypePowerSystem.SLOT_NAMES);
        SetupDeployableSlots(__instance);
        SetupFactorSlots(__instance);
    }

    private static void SetupDeployableSlots(uGUI_Equipment instance)
    {
        const float deployablesScale = 0.8f;
        var slot0 = CloneSlots(instance, DeployablesStorageTerminal.LightBeaconSlots, "BatteryCharger", null, DeployablesStorageTerminal.SLOT_POSITIONS, deployablesScale);
        CloneSlots(instance, new [] { DeployablesStorageTerminal.PHASE_GATE_SLOT } , "BatteryCharger", null,
            new [] { DeployablesStorageTerminal.PHASE_GATE_SLOT_POS });
        
        GameObject go = new();
        go.transform.SetParent(slot0.transform.parent);
        go.name = "DecoyStorageBackground";

        go.AddComponent<RectTransform>();
        
        go.transform.localScale = new Vector3(8, 10.3f, 1);
        go.AddComponent<CanvasRenderer>();
        var img = go.AddComponent<Image>();
        img.sprite = Plugin.AssetBundle.LoadAsset<Sprite>("Proto_DeployablesBG");
        img.raycastTarget = false;
        
        go.transform.SetParent(slot0.transform);
        go.transform.localPosition = new Vector3(190, 73, 0);
        
        GameObject storageAccess = GameObject.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("VehicleStorageAccess"), instance.transform);
        storageAccess.SetActive(false);
    }

    private static void SetupFactorSlots(uGUI_Equipment instance)
    {
        Vector3[] slotPositions = {
            new(-145, 225),
            new(-138, -238),
            new(145, 225),
            new(138, -238)
        };
        
        var slot0 = CloneSlots(instance, FactorEquipmentManager.FactorSlots , "BatteryCharger", null, slotPositions, 0.8f);
        GameObject go = new();
        go.transform.SetParent(slot0.transform);
        go.name = "FactorEquipmentBackground";

        go.AddComponent<RectTransform>();
        
        go.transform.localScale = new Vector3(12.5f, 12.5f, 1);
        go.transform.localPosition = new Vector3(181.25f, -281.25f, 0);
        go.AddComponent<CanvasRenderer>();
        var img = go.AddComponent<Image>();
        img.sprite = Plugin.AssetBundle.LoadAsset<Sprite>("Proto_Factor_2.2");
        img.raycastTarget = false;
        img.color = new Color(0.52f, 0.99f, 0.52f);
    }

#nullable enable
    private static uGUI_EquipmentSlot? CloneSlots(uGUI_Equipment equipment, string[] slots, string copyTarget = "SeamothModule", string? imageTarget = "Seamoth", Vector3[]? slotPositions = null, float scale = 1)
    {
        if (slots.Length == 0) return null;

        uGUI_EquipmentSlot slot = CloneSlot(equipment, $"{copyTarget}1", slots[0], scale);
        if (imageTarget != null)
        {
            GameObject.Destroy(slot.transform.Find(imageTarget).GetComponent<Image>());
        }

        if (slotPositions != null)
        {
            slot.transform.localPosition = slotPositions[0];
        }

        for (int i = 1; i < slots.Length; i++)
        {
            var clonedSlot = CloneSlot(equipment, $"{copyTarget}{Mathf.Min(4, i + 1)}", slots[i], scale);
            if (slotPositions != null)
            {
                clonedSlot.transform.localPosition = slotPositions[i];
            }
        }

        return slot;
    }
#nullable disable

    [HarmonyPatch(nameof(uGUI_Equipment.OnItemDragStart)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> OnItemDragStart_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatch match = new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "GetEquipmentType");

        FieldInfo inventoryItemInfo = typeof(Pickupable).GetField("inventoryItem", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo containerInfo = typeof(InventoryItem).GetField("container");

        var matcher = new CodeMatcher(instructions)
            .MatchForward(true, match)
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, inventoryItemInfo))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, containerInfo))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, inventoryItemInfo))
            .Insert(Transpilers.EmitDelegate(Inventory_Patches.GetModifiedEquipmentTypeItemsContainer));

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(nameof(uGUI_Equipment.CanSwitchOrSwap)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CanSwitchOrSwap_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatch match = new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "GetEquipmentType");

        FieldInfo containerInfo = typeof(InventoryItem).GetField("container");

        var matcher = new CodeMatcher(instructions)
            .MatchForward(true, match)
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, containerInfo))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
            .Insert(Transpilers.EmitDelegate(Inventory_Patches.GetModifiedEquipmentTypeItemsContainer));

        return matcher.InstructionEnumeration();
    }
    
    [HarmonyPatch(nameof(uGUI_Equipment.Init)), HarmonyPostfix]
    private static void Init_Postfix(uGUI_Equipment __instance, Equipment equipment)
    {
        bool isAccessButton = equipment._label == ProtoVehicleAccessTerminal.EQUIPMENT_LABEL;
        __instance.GetComponentInChildren<ProtoVehicleAccessManager>(true).gameObject.SetActive(isAccessButton);
        __instance.SendMessageUpwards("RefreshFactorSlots");
    }

    [HarmonyPatch(nameof(uGUI_Equipment.OnEquip)), HarmonyPrefix]
    private static void OnEquip_Prefix(uGUI_Equipment __instance)
    {
        __instance.SendMessageUpwards("RefreshFactorSlots");
    }
    
    [HarmonyPatch(nameof(uGUI_Equipment.OnUnequip)), HarmonyPrefix]
    private static void OnUnequip_Prefix(uGUI_Equipment __instance)
    {
        __instance.SendMessageUpwards("RefreshFactorSlots");
    }

    [HarmonyPatch(nameof(uGUI_Equipment.OnPointerClick)), HarmonyPrefix]
    private static bool OnPointerClick_Prefix(uGUI_EquipmentSlot instance)
    {
        if (instance.slot != "Body") return true;

        bool hasFactor = false;
        foreach (var slot in FactorEquipmentManager.FactorSlots)
        {
            if (Inventory.main.equipment.GetItemInSlot(slot) != null)
            {
                hasFactor = true;
                break;
            }
        }

        if (!hasFactor) return true;

        ErrorMessage.AddError(Language.main.Get("ProtoSuitUnequipWarning"));
        return false;
    }

    private static uGUI_EquipmentSlot CloneSlot(uGUI_Equipment equipmentMenu, string childName, string newSlotName, float scale)
    {
        Transform newSlot = GameObject.Instantiate(equipmentMenu.transform.Find(childName), equipmentMenu.transform);
        newSlot.transform.localScale = Vector3.one * scale;
        newSlot.name = newSlotName;
        uGUI_EquipmentSlot equipmentSlot = newSlot.GetComponent<uGUI_EquipmentSlot>();
        equipmentSlot.slot = newSlotName;
        return equipmentSlot;
    }

    private class DraggedItem
    {
        public Pickupable pickupable;
        public EquipmentType originalType;
        public DraggedItem(Pickupable pickupable, EquipmentType originalType)
        {
            this.pickupable = pickupable;
            this.originalType = originalType;
        }
    }
}
