using HarmonyLib;
using PrototypeSubMod.PowerSystem;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PrototypeSubMod.Factors;
using PrototypeSubMod.MiscMonobehaviors;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(Equipment))]
internal class Equipment_Patches
{
    [HarmonyPatch(nameof(Equipment.AllowedToAdd)), HarmonyPostfix]
    private static void AllowedToAdd_Postfix(Equipment __instance, Pickupable pickupable, ref bool __result)
    {
        if (pickupable != null && pickupable.TryGetComponent(out Factor factor))
        {
            __result = !Player.main.GetComponent<FactorManager>().ContainsFactor(factor);
            return;
        }
        
        if (!__instance.owner) return;

        if (__instance.typeToSlots.ElementAt(0).Key != Plugin.DummyPowerType) return;

        __result = PrototypePowerSystem.AllowedPowerSources.Keys.Contains(pickupable.GetTechType());
    }

    [HarmonyPatch("IItemsContainer.AddItem"), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> AddItem_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatch match = new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "GetEquipmentType");

        var matcher = new CodeMatcher(instructions)
            .MatchForward(true, match)
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .Insert(Transpilers.EmitDelegate(Inventory_Patches.GetModifiedEquipmentType));

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch("IItemsContainer.HasRoomFor"), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HasRoomFor_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatch match = new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "GetEquipmentType");

        var matcher = new CodeMatcher(instructions)
            .MatchForward(true, match)
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .Insert(Transpilers.EmitDelegate(Inventory_Patches.GetModifiedEquipmentType));

        return matcher.InstructionEnumeration();
    }
}
