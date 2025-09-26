using HarmonyLib;
using PrototypeSubMod.Upgrades;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PrecursorTeleporterCollider))]
public class PrecursorTeleporterCollider_Patches
{
    [HarmonyPatch(nameof(PrecursorTeleporterCollider.OnTriggerEnter)), HarmonyPostfix]
    private static void OnTriggerEnter_Postfix(PrecursorTeleporterCollider __instance, Collider col)
    {
        if (col.isTrigger) return;
        
        var subRoot = col.GetComponentInParent<SubRoot>();
        if (subRoot)
        {
            __instance.SendMessageUpwards("BeginTeleportSubRoot", subRoot,
                SendMessageOptions.DontRequireReceiver);
        }
    }
}