using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(DamagePlayerInRadius))]
public static class DamagePlayerInRadius_Patches
{
    [HarmonyPatch(nameof(DamagePlayerInRadius.DoDamage)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> DoDamage_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var gameObjectGetter = typeof(Component).GetProperty("gameObject").GetGetMethod();
        var match = new CodeMatch(i => i.opcode == OpCodes.Ldnull);

        var matcher = new CodeMatcher(instructions)
            .MatchForward(false, match)
            .SetAndAdvance(OpCodes.Ldarg_0, null)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt, gameObjectGetter));
        
        return matcher.InstructionEnumeration();
    }
}