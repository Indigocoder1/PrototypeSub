using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using PrototypeSubMod.Utility;
using Story;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(uGUI_RadioMessageIndicator))]
public class uGUI_RadioMessageIndicator_Patches
{
    [SaveStateReference]
    private static Dictionary<uGUI_RadioMessageIndicator, SpriteData> previousSpriteDatas;

    private static Dictionary<string, Sprite> radioMessageSprites = new();
    
    [HarmonyPatch(nameof(uGUI_RadioMessageIndicator.NewRadioMessage)), HarmonyPostfix]
    private static void NewRadioMessage_Postfix(uGUI_RadioMessageIndicator __instance, bool newMessages)
    {
        previousSpriteDatas ??= new Dictionary<uGUI_RadioMessageIndicator, SpriteData>();
        
        if (!newMessages) return;

        var mostRecentMessage = StoryGoalManager.main.pendingRadioMessages[^1];
        if (!mostRecentMessage.ToLower().Contains("proto")) return;

        previousSpriteDatas[__instance] = new SpriteData(__instance.sprite.sprite, __instance.sprite.color);
        Sprite sprite;
        if (!radioMessageSprites.TryGetValue(mostRecentMessage, out var messageSprite))
        {
            sprite = Plugin.AssetBundle.LoadAsset<Sprite>(mostRecentMessage);
        }
        else
        {
            sprite = messageSprite;
        }

        __instance.sprite.sprite = sprite;
        __instance.sprite.color = Color.white;
    }

    [HarmonyPatch(typeof(Player)), HarmonyPatch(nameof(Player.LateUpdate)), HarmonyPostfix]
    private static void LateUpdate_Postfix()
    {
        if (previousSpriteDatas.Count == 0) return;

        foreach (var indicator in previousSpriteDatas.Keys)
        {
            indicator.sprite.color = Color.white;
        }
    }

    [HarmonyPatch(nameof(uGUI_RadioMessageIndicator.DisableSprite)), HarmonyPostfix]
    private static void DisableSprite_Postfix(uGUI_RadioMessageIndicator __instance)
    {
        if (!previousSpriteDatas.TryGetValue(__instance, out var spriteData)) return;

        UWE.CoroutineHost.StartCoroutine(ResetSpriteDelayed(__instance, spriteData));
    }

    private static IEnumerator ResetSpriteDelayed(uGUI_RadioMessageIndicator __instance, SpriteData spriteData)
    {
        // To wait until the fade away animation finishes
        yield return new WaitForSeconds(0.1f);
        
        __instance.sprite.sprite = spriteData.sprite;
        __instance.sprite.color = spriteData.color;
        previousSpriteDatas.Remove(__instance);
    }

    private struct SpriteData
    {
        public Sprite sprite;
        public Color color;

        public SpriteData(Sprite sprite, Color color)
        {
            this.sprite = sprite;
            this.color = color;
        }
    }
}