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
        var currentBiome = (GetVolumeBiome(testPos) ?? LargeWorld.main.GetBiome(testPos)).ToLower();
        var inFacility = currentBiome.Contains("proto") && currentBiome.Contains("facility");
        
        return !inFacility;
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
}