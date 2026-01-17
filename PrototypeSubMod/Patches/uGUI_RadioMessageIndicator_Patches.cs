using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    [SaveStateReference]
    private static SpriteData defaultSpriteData;
    
    [HarmonyPatch(nameof(uGUI_RadioMessageIndicator.NewRadioMessage)), HarmonyPostfix]
    private static void NewRadioMessage_Postfix(uGUI_RadioMessageIndicator __instance, bool newMessages)
    {
        if (!StoryGoalManager.main || !StoryGoalManager.main.IsGoalComplete("OnPlayRadioBounceBack")) return;
        
        previousSpriteDatas ??= new Dictionary<uGUI_RadioMessageIndicator, SpriteData>();
        
        if (!newMessages) return;

        var nextMessage = StoryGoalManager.main.pendingRadioMessages[0];
        bool isProtoMessage = nextMessage.ToLower().Contains("proto");
        
        Sprite sprite = defaultSpriteData.sprite;
        Color color = defaultSpriteData.color;
        if (isProtoMessage)
        {
            sprite = Plugin.AssetBundle.LoadAsset<Sprite>(nextMessage);
            color = Color.white;
        }
        else if (previousSpriteDatas.TryGetValue(__instance, out var spriteData))
        {
            sprite = spriteData.sprite;
            color = spriteData.color;
        }
        
        previousSpriteDatas[__instance] = new SpriteData(__instance.sprite.sprite, __instance.sprite.color);

        __instance.sprite.sprite = sprite;
        __instance.sprite.color = color;
    }
    
    [HarmonyPatch(typeof(Player)), HarmonyPatch(nameof(Player.Awake)), HarmonyPrefix]
    private static void Awake_Prefix()
    {
        var messageIndicator = GameObject.FindObjectOfType<uGUI_RadioMessageIndicator>();
        defaultSpriteData = new SpriteData(messageIndicator.sprite.sprite, messageIndicator.sprite.color);
    }
    
    [HarmonyPatch(typeof(Player)), HarmonyPatch(nameof(Player.LateUpdate)), HarmonyPostfix]
    private static void LateUpdate_Postfix()
    {
        if (previousSpriteDatas == null || previousSpriteDatas.Count == 0) return;

        if (previousSpriteDatas.Values.ElementAt(0).sprite == defaultSpriteData.sprite) return;
        
        foreach (var indicator in previousSpriteDatas.Keys)
        {
            indicator.sprite.color = Color.white;
        }
    }

    [HarmonyPatch(nameof(uGUI_RadioMessageIndicator.DisableSprite)), HarmonyPostfix]
    private static void DisableSprite_Postfix(uGUI_RadioMessageIndicator __instance)
    {
        if (previousSpriteDatas == null) return;
        
        if (!previousSpriteDatas.TryGetValue(__instance, out var spriteData)) return;

        UWE.CoroutineHost.StartCoroutine(ResetSpriteDelayed(__instance, spriteData));
    }

    private static IEnumerator ResetSpriteDelayed(uGUI_RadioMessageIndicator __instance, SpriteData spriteData)
    {
        // To wait until the fade away animation finishes
        yield return new WaitForSeconds(0.1f);
        
        __instance.sprite.sprite = spriteData.sprite;
        __instance.sprite.color = spriteData.color;
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