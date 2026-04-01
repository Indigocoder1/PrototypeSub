using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PrototypeSubMod.DestructionEvent;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(RadiatePlayerInRange))]
public static class RadiatePlayerInRange_Patches
{
    [HarmonyPatch(nameof(RadiatePlayerInRange.Radiate)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Radiate_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = typeof(Player).GetMethod("SetRadiationAmount", AccessTools.all);
        var match = new CodeMatch(i => i.opcode == OpCodes.Callvirt && (MethodInfo)i.operand == method);

        var matcher = new CodeMatcher(instructions)
            .MatchForward(false, match)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(Transpilers.EmitDelegate(GetRadiationAmount));
        
        return matcher.InstructionEnumeration();
    }

    private static float GetRadiationAmount(float modifiedAmount, float originalAmount, RadiatePlayerInRange instance)
    {
        return instance.TryGetComponent(out IgnoreRadiationSuit _) ? originalAmount : modifiedAmount;
    }
}