using HarmonyLib;
using Story;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(VoidGhostLeviathansSpawner))]
public class VoidGhostLeviathanSpawnerPatch
{
    [HarmonyPatch(nameof(VoidGhostLeviathansSpawner.IsPlayerInVoid))]
    [HarmonyPrefix]
    private static bool IsPlayerInVoid_Prefix()
    {
        StoryGoalManager storyGoalManager = StoryGoalManager.main;
        return !storyGoalManager.IsGoalComplete("HullFacilityWormTerminalEncy") ||
               !storyGoalManager.IsGoalComplete("StartedCalibrationRun");
    }
}