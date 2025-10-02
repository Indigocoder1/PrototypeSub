using HarmonyLib;
using PrototypeSubMod.DeployablesTerminal;
using PrototypeSubMod.PowerSystem;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

        var slot0 = CloneSlots(__instance, DeployablesStorageTerminal.LightBeaconSlots, "BatteryCharger", null, DeployablesStorageTerminal.SLOT_POSITIONS);
        CloneSlots(__instance, new [] { DeployablesStorageTerminal.PHASE_GATE_SLOT } , "BatteryCharger", null,
            new [] { DeployablesStorageTerminal.PHASE_GATE_SLOT_POS }, slot0.transform);
        
        GameObject go = new();
        go.transform.SetParent(slot0.transform);
        go.name = "DecoyStorageBackground";

        go.AddComponent<RectTransform>();

        go.transform.localPosition = new Vector3(152, 57, 0);
        go.transform.localScale = new Vector3(8, 10.3f, 1);
        go.AddComponent<CanvasRenderer>();
        var img = go.AddComponent<Image>();
        img.sprite = Plugin.AssetBundle.LoadAsset<Sprite>("Proto_DeployablesBG");
        img.raycastTarget = false;
        
        GameObject storageAccess = GameObject.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("VehicleStorageAccess"), __instance.transform);
        storageAccess.SetActive(false);
    }

#nullable enable
    private static uGUI_EquipmentSlot? CloneSlots(uGUI_Equipment equipment, string[] slots, string copyTarget = "SeamothModule", string? imageTarget = "Seamoth", Vector3[]? slotPositions = null, Transform parent = null)
    {
        if (slots.Length == 0) return null;

        uGUI_EquipmentSlot slot = CloneSlot(equipment, $"{copyTarget}1", slots[0], parent);
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
            var clonedSlot = CloneSlot(equipment, $"{copyTarget}{Mathf.Min(4, i + 1)}", slots[i]);
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
    }

    private static uGUI_EquipmentSlot CloneSlot(uGUI_Equipment equipmentMenu, string childName, string newSlotName, Transform parent = null)
    {
        Transform newSlot = GameObject.Instantiate(equipmentMenu.transform.Find(childName), parent ?? equipmentMenu.transform);
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
