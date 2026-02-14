using HarmonyLib;
using Nautilus.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PDALog))]
internal class PDALog_Patches
{
    public static List<(string assetName, string key)> entries = new();
    public static List<(string assetName, string key)> orionEntries = new();
    
    [HarmonyPatch(nameof(PDALog.Initialize)), HarmonyPostfix]
    private static void Initialize_Postfix(PDAData pdaData)
    {
        Plugin.Logger.LogInfo($"Pda data = {pdaData}");
        Plugin.Logger.LogInfo($"Pda log = {pdaData.log}");
        Plugin.Logger.LogInfo($"Pda log item 0 = {pdaData.log[0]}");
        var pdaIcon = pdaData.log[0].icon;
        Plugin.Logger.LogInfo($"Pda Icon = {pdaIcon}");
        Plugin.Logger.LogInfo($"Asset bundle = {Plugin.GeneralAssetBundle}");
        Plugin.Logger.LogInfo($"Orion sprite = {Plugin.GeneralAssetBundle.LoadAsset<Sprite>("ProtoPDALogo")}");
        AddEntries(entries, pdaIcon);
        AddEntries(orionEntries, Plugin.GeneralAssetBundle.LoadAsset<Sprite>("ProtoPDALogo"));
    }

    private static void AddEntries(List<(string assetName, string key)> entriesToRegister, Sprite sprite)
    {
        foreach (var item in entriesToRegister)
        {
            var fmodAsset = AudioUtils.GetFmodAsset(item.assetName);
            fmodAsset.id = fmodAsset.path;

            PDALog.EntryData ency = new()
            {
                key = item.key,
                type = PDALog.EntryType.Default,
                icon = sprite,
                sound = fmodAsset,
                doNotAutoPlay = false
            };

            if (!PDALog.mapping.ContainsKey(item.key))
            {
                PDALog.mapping.Add(item.key, ency);
            }
        }
    }
}
