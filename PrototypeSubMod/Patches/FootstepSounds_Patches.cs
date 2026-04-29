using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(FootstepSounds))]
public static class FootstepSounds_Patches
{
    [HarmonyPatch(nameof(FootstepSounds.OnStep)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> OnStep_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var outOfWaterField = typeof(Player).GetField("precursorOutOfWater", BindingFlags.Public | BindingFlags.Instance);
        var match = new CodeMatch(i => i.opcode == OpCodes.Ldfld && (FieldInfo)i.operand == outOfWaterField);

        var matcher = new CodeMatcher(instructions)
            .MatchForward(false, match)
            .Advance(1)
            .InsertAndAdvance(Transpilers.EmitDelegate(ReplaceOutOfWater));

        return matcher.InstructionEnumeration();
    }

    private static bool ReplaceOutOfWater(bool prevOutOfWater)
    {
        return AtmosphereDirector.main.GetBiomeOverride() != "protohulloutpost" && prevOutOfWater;
    }
}