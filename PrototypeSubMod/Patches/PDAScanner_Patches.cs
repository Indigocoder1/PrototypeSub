using System;
using HarmonyLib;
using Story;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PDAScanner))]
public static class PDAScanner_Patches
{
    private static TechType SparseReefWorm;
    private static TechType GrassyPlateausWorm;
    
    [HarmonyPatch(nameof(PDAScanner.Scan)), HarmonyPostfix]
    private static void Scan_Postfix()
    {
        if (SparseReefWorm == TechType.None)
        {
            SparseReefWorm = (TechType)Enum.Parse(typeof(TechType), "ProtoSparseReefWyrm");
        }
        
        if (GrassyPlateausWorm == TechType.None)
        {
            GrassyPlateausWorm = (TechType)Enum.Parse(typeof(TechType), "ProtoGrassyWyrm");
        }

        var techType = PDAScanner.scanTarget.techType;
        if (techType != SparseReefWorm && techType != GrassyPlateausWorm) return;
        
        if (!PDAScanner.complete.Contains(techType)) return;

        StoryGoalManager.main.OnGoalComplete("OnShallowWyrmScanned");
    }
}