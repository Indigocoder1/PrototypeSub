using HarmonyLib;
using PrototypeSubMod.Prefabs;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(WarpBall))]
public class WarpBall_Patch : MonoBehaviour
{
    [HarmonyPatch(nameof(WarpBall.Warp)), HarmonyPrefix]
    private static bool Warp_Prefix(WarpBall __instance, GameObject target)
    {
        if (Player.main.gameObject != target) return true;

        for (int i = 1; i <= 2; i++)
        {
            var itemInSlot = Inventory.main.equipment.GetItemInSlot($"Chip{i}");
            if (itemInSlot == null) continue;
            
            if (itemInSlot.techType != ProtoPhaseGateStabilizer.PrefabInfo.TechType) continue;

            Destroy(__instance.gameObject);
            return false;
        }

        return true;
    }
}