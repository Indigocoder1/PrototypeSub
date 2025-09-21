using System;
using HarmonyLib;
using PrototypeSubMod.LightDistortionField;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PlayerWorldArrows))]
public class PlayerWorldArrows_Patches
{
    [HarmonyPatch(nameof(PlayerWorldArrows.CreateWorldArrows)), HarmonyPostfix]
    private static void CreateWorldArrows_Postfix(PlayerWorldArrows __instance)
    {
        var radialWheelTT = (TechType)Enum.Parse(typeof(TechType), "ProtoRadialWheel");
        __instance.CreateWorldArrow(false, false, radialWheelTT, "ProtoRadialHint",
            null, "ProtoOpenRadialWheel", 0, new Vector3(0, -120, 0), true, localScale: 150f);

        __instance.worldArrows[^1].gameConditionDelegate = (ref Transform transform) =>
        {
            if (Player.main.GetMode() != Player.Mode.Piloting) return false;
            
            foreach (var effectHandler in CloakEffectHandler.EffectHandlers)
            {
                var subRoot = effectHandler.GetComponentInParent<SubRoot>();
                if (subRoot && subRoot.GetComponent<CyclopsMotorMode>().engineOn)
                {
                    transform = subRoot.transform.Find("PrototypeHUD/MiddleStatus/RadialHintTarget");
                    return true;
                }
            }

            return false;
        };
    }
}