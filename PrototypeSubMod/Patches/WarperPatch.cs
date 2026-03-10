using HarmonyLib;
using PrototypeSubMod.Prefabs.AlienBuildingBlock;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(Warper))]
public class WarperPatch
{
    [HarmonyPatch(nameof(Warper.EndWarpOut))]
    [HarmonyPrefix]
    public static void EndWarpOut_Prefix(Warper __instance)
    {
        RollBuildingBlockSpawnChance(__instance.transform.position);
    }

    private static void RollBuildingBlockSpawnChance(Vector3 spawnPos)
    {
        CoroutineHost.StartCoroutine(WarperRemnant.TrySpawnBiome(spawnPos,
                LargeWorld.main.GetBiome(spawnPos)));
    }
    
}