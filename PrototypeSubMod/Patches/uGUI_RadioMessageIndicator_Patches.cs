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
    private static Dictionary<uGUI_RadioMessageIndicator, SpriteData> _previousSpriteData;
    
    [HarmonyPatch(nameof(uGUI_RadioMessageIndicator.NewRadioMessage)), HarmonyPostfix]
    private static void NewRadioMessage_Postfix(uGUI_RadioMessageIndicator __instance, bool newMessages)
    {
        if (!newMessages) return;
        
        if (!StoryGoalManager.main || !StoryGoalManager.main.IsGoalComplete("OnPlayRadioBounceBack")) return;
        
        _previousSpriteData ??= new Dictionary<uGUI_RadioMessageIndicator, SpriteData>();

        ResetSprite(__instance);

        _previousSpriteData[__instance] = new SpriteData(__instance.sprite.sprite, __instance.sprite.color);
        
        if (StoryGoalManager.main.pendingRadioMessages.Count == 0) return;
        
        var nextMessage = StoryGoalManager.main.pendingRadioMessages[0];
        bool isProtoMessage = nextMessage.ToLower().Contains("proto");
        if (!isProtoMessage) return;
        
        var sprite = Plugin.GeneralAssetBundle.LoadAsset<Sprite>(nextMessage);
        __instance.sprite.sprite = sprite;
        __instance.sprite.color = Color.white;
    }

    [HarmonyPatch(nameof(uGUI_RadioMessageIndicator.DisableSprite)), HarmonyPostfix]
    private static void DisableSprite_Postfix(uGUI_RadioMessageIndicator __instance)
    {
        ResetSprite(__instance);
    }

    private static void ResetSprite(uGUI_RadioMessageIndicator instance)
    {
        if (_previousSpriteData == null) return;
        
        if (!_previousSpriteData.TryGetValue(instance, out var spriteData)) return;

        UWE.CoroutineHost.StartCoroutine(ResetSpriteDelayed(instance, spriteData));

        _previousSpriteData.Remove(instance);
    }

    private static IEnumerator ResetSpriteDelayed(uGUI_RadioMessageIndicator instance, SpriteData spriteData)
    {
        // To wait until the fade away animation finishes
        yield return new WaitForSeconds(0.1f);
        
        instance.sprite.sprite = spriteData.sprite;
        instance.sprite.color = spriteData.color;
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