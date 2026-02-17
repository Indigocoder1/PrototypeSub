using HarmonyLib;
using PrototypeSubMod.PowerSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PrototypeSubMod.Factors;
using PrototypeSubMod.PrecursorWearables;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.Prefabs.AlienBuildingBlock;
using PrototypeSubMod.Prefabs.Factors;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(Inventory))]
internal class Inventory_Patches
{
    [HarmonyPatch(nameof(Inventory.AddOrSwap)), HarmonyTranspiler]
    [HarmonyPatch(new [] { typeof(InventoryItem), typeof(Equipment), typeof(string) })]
    private static IEnumerable<CodeInstruction> AddOrSwap_Equipment_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatch match = new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "GetEquipmentType");

        var matcher = new CodeMatcher(instructions)
            .MatchForward(true, match)
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .Insert(Transpilers.EmitDelegate(GetModifiedEquipmentType));

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(nameof(Inventory.GetAllItemActions)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> GetAllItemActions_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatch match = new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "GetEquipmentType");

        MethodInfo methodInfo = typeof(Inventory).GetMethod("GetOppositeContainer");

        var matcher = new CodeMatcher(instructions)
            .MatchForward(true, match)
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)) //Load the Inventory instance (this)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1)) //Load the InventoryItem
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, methodInfo)) //Get the container on the right side of the inventory
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1)) //Load the InventoryItem
            .Insert(Transpilers.EmitDelegate(GetModifiedEquipmentTypeItemsContainer));

        return matcher.InstructionEnumeration();
    }

    public static EquipmentType GetModifiedEquipmentType(EquipmentType originalType, InventoryItem itemA, Equipment equipmentB)
    {
        if (itemA == null) return originalType;

        if (!equipmentB.owner) return originalType;

        bool isPowerEquipment = equipmentB.typeToSlots.ElementAt(0).Key == Plugin.DummyPowerType;
        if (!isPowerEquipment) return originalType;

        if (PrototypePowerSystem.AllowedPowerSources.Keys.Contains(itemA.techType))
        {
            return Plugin.PrototypePowerType;
        }

        return originalType;
    }

    public static EquipmentType GetModifiedEquipmentTypeItemsContainer(EquipmentType originalType, IItemsContainer container, InventoryItem itemA)
    {
        if (itemA == null) return originalType;
        
        bool transferContainer = container.label != PrototypePowerSystem.EquipmentLabel;

        if (transferContainer)
        {
            return originalType;
        }

        if (PrototypePowerSystem.AllowedPowerSources.Keys.Contains(itemA.techType))
        {
            return Plugin.PrototypePowerType;
        }

        return originalType;
    }
    
    [HarmonyPatch(nameof(Inventory.AddOrSwap)), HarmonyPrefix]
    [HarmonyPatch(new [] { typeof(InventoryItem), typeof(Equipment), typeof(string) })]
    private static bool AddOrSwap_Equipment_Prefix(InventoryItem itemA, Equipment equipmentB, string slotB, ref bool __result)
    {
        if (string.IsNullOrEmpty(slotB))
        {
            var equipmentType = TechData.GetEquipmentType(itemA.item.GetTechType());
            equipmentB.GetCompatibleSlot(equipmentType, out slotB);
        }

        if (string.IsNullOrEmpty(slotB)) return true;
        
        var itemB = equipmentB.GetItemInSlot(slotB);
        var tt = itemB?.techType ?? TechType.None;
        if (itemB == null) return true;

        if (itemB.techType == PrecursorSuit.prefabInfo.TechType && itemA.techType != PrecursorSuit.prefabInfo.TechType)
        {
            ErrorMessage.AddError(Language.main.Get("ProtoSuitUnequipWarning"));
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPatch(nameof(Inventory.Awake)), HarmonyPostfix]
    private static void Awake_Postfix(Inventory __instance)
    {
        var glovesManager = __instance.gameObject.EnsureComponent<PropulsionGlovesManager>();
        __instance.quickSlots.onSelect += _ => glovesManager.UpdateToolActive();
        __instance.equipment.onEquip += (_, _) => glovesManager.UpdateToolActive();
        __instance.equipment.onUnequip += (_, _) => glovesManager.UpdateToolActive();
    }

    [HarmonyPatch(nameof(Inventory.ExecuteItemAction)), HarmonyPrefix]
    [HarmonyPatch(new [] { typeof(ItemAction), typeof(InventoryItem) })]
    private static void ExecuteItemAction_Prefix(ItemAction action, InventoryItem item)
    {
        if (action != ItemAction.Eat || !item.item.TryGetComponent(out BiomechanicsEatable eatable)) return;

        eatable.OnEat();
    }
    
    [HarmonyPatch(nameof(Inventory.GetAllItemActions)), HarmonyPostfix]
    private static void GetAllItemActions_Postfix(InventoryItem item, ref ItemAction __result)
    {
        if (item.techType != AlienBuildingBlock.prefabInfo.TechType) return;

        var itemInBody = Inventory.main.equipment.GetItemInSlot("Body");
        if (itemInBody == null || itemInBody.techType != PrecursorSuit.prefabInfo.TechType) return;
        
        if ((__result & ItemAction.Switch) > 0) return;
        
        __result |= ItemAction.Eat;
    }

    [HarmonyPatch(nameof(Inventory.InternalDropItem)), HarmonyPrefix]
    private static void InternalDropItem_Prefix(Pickupable pickupable)
    {
        pickupable.inventoryItem ??= new InventoryItem(pickupable);
        
        if (pickupable.inventoryItem.techType != PrecursorSuit.prefabInfo.TechType) return;

        var factorManager = uGUI_PDA.main.GetComponentInChildren<FactorEquipmentManager>(true);
        
        foreach (var slot in FactorEquipmentManager.FactorSlots)
        {
            var item = Inventory.main.equipment.GetItemInSlot(slot);
            if (item == null) continue;
            
            Inventory.main.equipment.RemoveItem(item.item);
            Inventory.main.InternalDropItem(item.item);
        }

        factorManager.RefreshFactorSlots();
    }
}
