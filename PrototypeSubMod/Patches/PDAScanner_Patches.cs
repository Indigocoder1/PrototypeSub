using System;
using HarmonyLib;
using PrototypeSubMod.Prefabs.DecorativeWyrms;
using Story;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PDAScanner))]
public static class PDAScanner_Patches
{
    [HarmonyPatch(nameof(PDAScanner.Scan)), HarmonyPrefix]
    private static void Scan_Prefix(out (TechType type, bool wasUnlocked) __state)
    {
        __state.type = PDAScanner.scanTarget.techType;
        __state.wasUnlocked = PDAScanner.complete.Contains(__state.type);
    }
    
    [HarmonyPatch(nameof(PDAScanner.Scan)), HarmonyPostfix]
    private static void Scan_Postfix((TechType type, bool wasUnlocked) __state)
    {
        if (WasJustUnlocked(ProtoSparseReefWyrms.prefabInfo.TechType, __state.wasUnlocked))
        {
            StoryGoalManager.main.OnGoalComplete("OnScannedSparseReefWyrms");
            ScanUnlockedWyrm();
        }
 
        if (WasJustUnlocked(ProtoGrandReefWyrms.prefabInfo.TechType, __state.wasUnlocked))
        {
            PDALog.Add("OnScanDisabledWyrm");
            ScanUnlockedWyrm();
        }

        if (WasJustUnlocked(ProtoGrassyWyrms.prefabInfo.TechType, __state.wasUnlocked))
        {
            StoryGoalManager.main.OnGoalComplete("OnScannedGrassyWyrms");
            ScanUnlockedWyrm();
        }
    }

    private static bool WasJustUnlocked(TechType type, bool wasUnlocked)
    {
        var scannerType = PDAScanner.scanTarget.techType;
        return scannerType == type && PDAScanner.complete.Contains(scannerType) && !wasUnlocked;
    }

    private static void ScanUnlockedWyrm()
    {
        var techType = ProtoUnlockedWyrm.prefabInfo.TechType;
        PDAScanner.Entry entry;
        if (!PDAScanner.GetPartialEntryByKey(techType, out entry))
        {
            entry = PDAScanner.Add(techType, 0);
        }
        
        var entryData = PDAScanner.GetEntryData(techType);
        if (entry == null) return;
        
        Plugin.Logger.LogInfo($"Scanning unlocked wyrm. Entry = {entry} | Total fragments = {entryData.totalFragments} | Unlocked = {entry.unlocked}");
        
        entry.unlocked++;
        if (entry.unlocked >= entryData.totalFragments)
        {
            PDAScanner.partial.Remove(entry);
            PDAScanner.complete.Add(entry.techType);
            PDAScanner.NotifyRemove(entry);
            PDAScanner.Unlock(entryData, true, true);
        }
        else
        {
            var percentCompletion = (float)Mathf.RoundToInt(entry.unlocked / (float)entryData.totalFragments * 100f);
            ErrorMessage.AddError(Language.main.GetFormat("ScannerInstanceScanned",
                Language.main.Get(techType.AsString()), percentCompletion, entry.unlocked, entryData.totalFragments));
        }
    }
}