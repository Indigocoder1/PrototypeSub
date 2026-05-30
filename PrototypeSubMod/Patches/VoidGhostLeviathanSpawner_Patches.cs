using HarmonyLib;
using Story;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(VoidGhostLeviathansSpawner))]
public class VoidGhostLeviathanSpawnerPatch
{
    [HarmonyPatch(nameof(VoidGhostLeviathansSpawner.UpdateSpawn))]
    [HarmonyPrefix]
    private static bool UpdateSpawn_Prefix()
    {
        StoryGoalManager storyGoalManager = StoryGoalManager.main;
        return !storyGoalManager.IsGoalComplete("HullFacilityWormTerminalEncy");
    }
    
    [HarmonyPatch(nameof(VoidGhostLeviathansSpawner.IsPlayerInVoid))]
    [HarmonyPrefix]
    private static bool IsPlayerInVoid_Prefix()
    {
        StoryGoalManager storyGoalManager = StoryGoalManager.main;
        return !storyGoalManager.IsGoalComplete("HullFacilityWormTerminalEncy");
    }
}