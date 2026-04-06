using System;
using HarmonyLib;
using PrototypeSubMod.LightDistortionField;
using PrototypeSubMod.Teleporter;
using UnityEngine;
using Story;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PlayerWorldArrows))]
public class PlayerWorldArrows_Patches
{
    [HarmonyPatch(nameof(PlayerWorldArrows.CreateWorldArrows)), HarmonyPostfix]
    private static void CreateWorldArrows_Postfix(PlayerWorldArrows __instance)
    {
        CreateRadialWheelArrow(__instance);
        CreateInterceptorMapArrow(__instance);
    }

    private static void CreateRadialWheelArrow(PlayerWorldArrows instance)
    {
        var radialWheelTT = (TechType)Enum.Parse(typeof(TechType), "ProtoRadialWheel");
        instance.CreateWorldArrow(false, false, radialWheelTT, "ProtoRadialHint",
            null, "ProtoOpenRadialWheel", 0, new Vector3(0, -120, 0), true, localScale: 150f);

        instance.worldArrows[^1].gameConditionDelegate = (ref Transform transform) =>
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

    private static void CreateInterceptorMapArrow(PlayerWorldArrows instance)
    {
        if (StoryGoalManager.main.IsGoalComplete("ProtoOpenInterceptorMap")) return;
        
        var interceptorMapTT = (TechType)Enum.Parse(typeof(TechType), "ProtoInterceptorMap");
        instance.CreateWorldArrow(false, false, interceptorMapTT, "ProtoInterceptorMapHint",
            null, "ProtoOpenInterceptorMap", 0, new Vector3(0, -0.2f, 0), true, localScale: 0.75f);

        instance.worldArrows[^1].gameConditionDelegate = (ref Transform transform) =>
        {
            if (Player.main.currentSub == null) return false;
            
            foreach (var effectHandler in CloakEffectHandler.EffectHandlers)
            {
                var subRoot = effectHandler.GetComponentInParent<SubRoot>();
                if (Player.main.currentSub != subRoot) continue;

                var teleporterManager = subRoot.GetComponentInChildren<ProtoTeleporterManager>();
                if (!teleporterManager.GetUpgradeInstalled()) continue;

                transform = teleporterManager.transform.Find("MapOpenHint");

                if (!StoryGoalManager.main.IsGoalComplete(("ArchwayOverrideHint")))
                {
                    StoryGoalManager.main.OnGoalComplete(("ArchwayOverrideHint"));
                }
                return true;
            }

            return false;
        };
    }
}