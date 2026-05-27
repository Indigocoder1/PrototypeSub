using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PrefabPlaceholder))]
public static class PrefabPlaceholder_Patches
{
    private static readonly List<string> WhitelistedClassIDs = new()
    {
        "38ebd2e5-9dcc-4d7a-ada4-86a22e01191a", // Ion cubes
        "41406e76-4b4c-4072-86f8-f5b8e6523b73" // Drillable ion cubes
    };
    
    [HarmonyPatch(nameof(PrefabPlaceholder.Spawn)), HarmonyPrefix]
    private static bool Spawn_Prefix(PrefabPlaceholder __instance)
    {
        if (WhitelistedClassIDs.Contains(__instance.prefabClassId)) return true;
        
        var testPos = __instance.transform.position;
        
        return !InFacilityBiome(testPos);
    }

    private static string GetVolumeBiome(Vector3 worldPosition)
    {
        if (AtmosphereDirector.main == null) return null;
        
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
        if (LargeWorld.main == null) return false;
        
        var currentBiome = (GetVolumeBiome(worldPosition) ?? LargeWorld.main.GetBiome(worldPosition))?.ToLower();
        if (currentBiome == null) return false;
        return currentBiome.Contains("proto") && currentBiome.Contains("facility");
    }
}