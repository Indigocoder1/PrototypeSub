using System;
using System.Collections.Generic;
using HarmonyLib;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PDAEncyclopedia))]
public class PDAEncyclopedia_Patches
{
    public static Dictionary<string, Action> EncyclopediaUnlockEvents = new();
    
    [HarmonyPatch(nameof(PDAEncyclopedia.Add), typeof(string), typeof(PDAEncyclopedia.Entry), typeof(bool)), HarmonyPostfix]
    private static void Add_Postfix(string key)
    {
        if (EncyclopediaUnlockEvents.TryGetValue(key, out var action))
        {
            action();
        }
    }
}