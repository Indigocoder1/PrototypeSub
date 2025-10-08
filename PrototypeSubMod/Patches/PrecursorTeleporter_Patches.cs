using HarmonyLib;
using PrototypeSubMod.Teleporter;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PrecursorTeleporter))]
internal class PrecursorTeleporter_Patches
{
    private static string lastTeleporterID;
    private static bool lastTeleporterWasProtoSub;

    [HarmonyPatch(nameof(PrecursorTeleporter.Start)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Start_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var exitPointField = typeof(PrecursorTeleporter).GetField("registerPrisonExitPoint", BindingFlags.Public | BindingFlags.Instance);
        var fieldMatch = new CodeMatch(i => i.opcode == OpCodes.Ldfld && (FieldInfo)i.operand == exitPointField);

        var labelMatcher = new CodeMatcher(instructions)
            .MatchForward(false, fieldMatch)
            .Advance(3);

        var label = labelMatcher.Instruction.operand;
        var matcher = new CodeMatcher(instructions)
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(Transpilers.EmitDelegate(HasValuesInitialized))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse_S, label));

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(nameof(PrecursorTeleporter.TeleportRoutine)), HarmonyPostfix]
    private static void TeleportRoutine_Postfix(PrecursorTeleporter __instance)
    {
        if (!__instance.enabled || __instance == null) return;
        
        lastTeleporterID = __instance.teleporterIdentifier;
        lastTeleporterWasProtoSub = __instance.TryGetComponent(out ProtoTeleporterManager positionSetter);

        Plugin.Logger.LogInfo($"Teleport routine postfix from {__instance} | Was proto sub = {lastTeleporterWasProtoSub}");
        if (lastTeleporterWasProtoSub)
        {
            lastTeleporterID = positionSetter.GetTeleporterIDNoIndicator();
        }
    }

    [HarmonyPatch(nameof(PrecursorTeleporter.TeleportationComplete)), HarmonyPostfix]
    private static void TeleportationComplete_Postfix(PrecursorTeleporter __instance)
    {
        Plugin.Logger.LogInfo($"last teleporter ID = {lastTeleporterID}");
        
        if (!lastTeleporterWasProtoSub) return;

        if (TeleporterPositionHandler.OutOfWaterTeleporters.Contains(lastTeleporterID))
        {
            Player.main.SetPrecursorOutOfWater(true);
        }
        else
        {
            Player.main.SetPrecursorOutOfWater(false);
        }
    }

    public static bool HasValuesInitialized(PrecursorTeleporter instance)
    {
        if (instance.portalFxPrefab == null) return false;

        return true;
    }
}
