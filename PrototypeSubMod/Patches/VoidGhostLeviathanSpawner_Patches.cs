using HarmonyLib;
using Nautilus.Handlers;
using Story;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(VoidGhostLeviathansSpawner))]
public class VoidGhostLeviathanSpawnerPatch
{
    [HarmonyPatch(nameof(VoidGhostLeviathansSpawner.IsPlayerInVoid))]
    [HarmonyPrefix]
    public static bool CheckWormActivationGoal()
    {
        StoryGoalManager storyGoalManager = StoryGoalManager.main;
        if (storyGoalManager.IsGoalComplete("HullFacilityActivateWorm"))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}