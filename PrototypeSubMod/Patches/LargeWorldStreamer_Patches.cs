using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(LargeWorldStreamer))]
internal class LargeWorldStreamer_Patches
{
    [SaveStateReference(false)]
    private static bool _overwrite;
    [SaveStateReference]
    private static Vector3 _overwriteCamPos;

    [HarmonyPatch(nameof(LargeWorldStreamer.Start)), HarmonyPatch(MethodType.Enumerator), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Start_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var getPosCall = typeof(Transform).GetProperty("position", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();

        var matches = new[]
        {
            new CodeMatch(i => i.opcode == OpCodes.Ldloc_1),
            new CodeMatch(i => i.opcode == OpCodes.Ldloc_S),
            new CodeMatch(i => i.opcode == OpCodes.Callvirt)
        };

        var matcher = new CodeMatcher(instructions)
            .MatchForward(true, matches)
            .Advance(1)
            .InsertAndAdvance(Transpilers.EmitDelegate(OverwriteCamPosIfNeeded));

        return matcher.InstructionEnumeration();
    }

    public static Vector3 OverwriteCamPosIfNeeded(Vector3 camPos)
    {
        return _overwrite ? _overwriteCamPos : camPos;
    }

    public static void SetOverwriteCamPos(bool overwrite, Vector3 position)
    {
        _overwrite = overwrite;
        _overwriteCamPos = position;
    }
}
