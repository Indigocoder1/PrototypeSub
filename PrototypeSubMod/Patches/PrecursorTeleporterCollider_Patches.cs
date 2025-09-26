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
        
        var upgradeManager = col.GetComponentInParent<ProtoUpgradeManager>();
        if (upgradeManager)
        {
            __instance.SendMessageUpwards("BeginTeleportPrototype", upgradeManager,
                SendMessageOptions.DontRequireReceiver);
        }
    }
}