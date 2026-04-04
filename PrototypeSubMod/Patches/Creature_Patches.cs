using HarmonyLib;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(Creature))]
public class Creature_Patches
{
    [HarmonyPatch(nameof(Creature.Start)), HarmonyPrefix]
    private static bool Start_Prefix(Creature __instance)
    {
        var isLeviathan = __instance is ReaperLeviathan or GhostLeviathan or SeaTreader;
        if (PrefabPlaceholder_Patches.InFacilityBiome(__instance.transform.position) && !isLeviathan)
        {
            GameObject.Destroy(__instance.gameObject);
            return false;
        }
        
        if (__instance is not ReaperLeviathan) return true;

        var identifier = __instance.GetComponent<UniqueIdentifier>();
        if (!identifier) return true;

        // Remove that one Reaper near the defense moonpool door
        if (identifier.Id == "d679ef73-3f07-495f-9f06-688123e3e806")
        {
            GameObject.Destroy(__instance.gameObject);
            return false;
        }

        return true;
    }
}