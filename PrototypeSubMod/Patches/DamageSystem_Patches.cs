using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PrototypeSubMod.DestructionEvent;
using PrototypeSubMod.IonBarrier;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(DamageSystem))]
internal class DamageSystem_Patches
{
    [HarmonyPatch(nameof(DamageSystem.CalculateDamage)), HarmonyPostfix]
    private static void CalculateDamage_Postfix(ref float __result, DamageType type, GameObject target)
    {
        var ionBarrier = target.GetComponentInChildren<ProtoIonBarrier>(true);
        if (ionBarrier == null) return;

        if (!ionBarrier.GetUpgradeEnabled() || !ionBarrier.GetUpgradeInstalled()) return;

        __result *= ionBarrier.GetReductionForType(type);
    }

    [HarmonyPatch(nameof(DamageSystem.CalculateDamage)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CalculateDamage_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = typeof(Player).GetMethod("HasReinforcedSuit", AccessTools.all);
        var match = new CodeMatch(i => i.opcode == OpCodes.Callvirt && (MethodInfo)i.operand == method);
        var match2 = new CodeMatch(i => i.opcode == OpCodes.Ldloc_1);
        
        var matcher = new CodeMatcher(instructions)
            .MatchForward(false, match)
            .MatchBack(false, match2)
            .Advance(2)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 6))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_3))
            .InsertAndAdvance(Transpilers.EmitDelegate(GetDamageOverride))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Starg_S, 0));
        
        return matcher.InstructionEnumeration();
    }

    private static float GetDamageOverride(float modified, float original, DamageType damageType, GameObject dealer)
    {
        if (damageType != DamageType.Radiation) return modified;

        if (dealer == null || !dealer.TryGetComponent(out IgnoreRadiationSuit _)) return modified;

        return original;
    }
}
