using System.Diagnostics;
using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PrefabPlaceholder))]
public static class PrefabPlaceholder_Patches
{
    [HarmonyPatch(nameof(PrefabPlaceholder.Spawn)), HarmonyPrefix]
    private static bool Spawn_Prefix(PrefabPlaceholder __instance)
    {
        var testPos = __instance.transform.position;
        
        return !InFacilityBiome(testPos);
    }

    private static string GetVolumeBiome(Vector3 worldPosition)
    {
        foreach (var volume in AtmosphereDirector.main.GetVolumes())
        {
            if (volume.overrideBiome == string.Empty) continue;
            
            if (!volume.collider.bounds.Contains(worldPosition)) continue;
            
            return volume.overrideBiome;
        }

        return null;
    }

    public static bool InFacilityBiome(Vector3 worldPosition)
    {
        var currentBiome = (GetVolumeBiome(worldPosition) ?? LargeWorld.main.GetBiome(worldPosition)).ToLower();
        return currentBiome.Contains("proto") && currentBiome.Contains("facility");
    }
}