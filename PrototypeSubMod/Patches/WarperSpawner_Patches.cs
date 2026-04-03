using HarmonyLib;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(WarperSpawner))]
public static class WarperSpawner_Patches
{
    [HarmonyPatch(nameof(WarperSpawner.OnEnable)), HarmonyPostfix]
    private static void OnEnable_Postfix(WarperSpawner __instance)
    {
        if (!PrefabPlaceholder_Patches.InFacilityBiome(__instance.transform.position)) return;

        GameObject.Destroy(__instance.gameObject);
    }
}