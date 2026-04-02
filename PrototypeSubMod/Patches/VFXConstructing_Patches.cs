using System.Linq;
using HarmonyLib;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(VFXConstructing))]
public static class VFXConstructing_Patches
{
    [HarmonyPatch(nameof(VFXConstructing.Regenerate)), HarmonyPostfix]
    private static void Regenerate_Postfix(VFXConstructing __instance)
    {
        var newRends = __instance.renderers.ToList();
        foreach (var rend in __instance.renderers)
        {
            if (rend.TryGetComponent(out ExcludeFromVFXConstructing _))
            {
                newRends.Remove(rend);
            }
        }

        __instance.renderers = newRends.ToArray();
    }
}