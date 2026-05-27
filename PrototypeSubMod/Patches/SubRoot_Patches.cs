using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.MotorHandler;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(SubRoot))]
internal class SubRoot_Patches
{
    [HarmonyPatch(nameof(SubRoot.UpdatePowerRating)), HarmonyPostfix]
    private static void UpdatePowerRating_Postfix(SubRoot __instance)
    {
        var motorHandler = __instance.GetComponentInChildren<ProtoMotorHandler>(true);
        if (!motorHandler) return;

        __instance.currPowerRating *= motorHandler.GetEfficiencyMultiplier();
    }

    [HarmonyPatch(nameof(SubRoot.OnPlayerEntered)), HarmonyPrefix]
    private static void OnPlayerEnter_Prefix(SubRoot __instance)
    {
        if (!__instance.voiceNotificationManager) return;
        
        __instance.voiceNotificationManager.ClearQueue();
    }
    
    [HarmonyPatch(nameof(SubRoot.OnPlayerEntered)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> OnPlayerEnter_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var playNotificationMethod =
            typeof(VoiceNotificationManager).GetMethod("PlayVoiceNotification", AccessTools.all);

        var match = new CodeMatch[]
        {
            new(i => i.opcode == OpCodes.Ldc_I4_1),
            new(i => i.opcode == OpCodes.Ldc_I4_0),
            new(i => i.opcode == OpCodes.Callvirt && (MethodInfo)i.operand == playNotificationMethod)
        };

        var matcher = new CodeMatcher(instructions)
            .MatchForward(false, match)
            .SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_0))
            .MatchForward(false, match)
            .SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_0))
            .MatchForward(false, match)
            .SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_0));
        
        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(nameof(SubRoot.UpdateLighting)), HarmonyPrefix]
    private static bool UpdateLighting_Prefix(SubRoot __instance)
    {
        var lightingControllerManager = __instance.GetComponentInChildren<LightingControllerManager>();
        if (lightingControllerManager == null) return true;

        return !lightingControllerManager.ManualLightControlActive();
    }
}
