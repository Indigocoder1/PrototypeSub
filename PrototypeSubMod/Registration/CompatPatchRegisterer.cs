using System;
using BepInEx.Bootstrap;
using HarmonyLib;
using PrototypeSubMod.Patches;

namespace PrototypeSubMod.Registration;

public static class CompatPatchRegisterer
{
    public static void RegisterCompatibilityPatches(Harmony harmony)
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        
        if (Chainloader.PluginInfos.ContainsKey("com.danithedani.deepercreatures"))
        {
            var structureLoadingType =
                Type.GetType(
                    "EpicStructureLoader.StructureLoading, EpicStructureLoader, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null");
            var structureTranspiler = AccessTools.Method(typeof(StructureLoading_Patches), nameof(StructureLoading_Patches.RegisterStructure_Transpiler));
            var originalMethod = AccessTools.Method(structureLoadingType, "RegisterStructure");
            harmony.Patch(originalMethod, transpiler: new HarmonyMethod(structureTranspiler));
        }
        
        if (Chainloader.PluginInfos.ContainsKey("com.aci.thesilence"))
        {
            var grabType = AccessTools.TypeByName("CallOfTheVoid.Mono.Creatures.Silence.SilenceCyclopsGrab");
            var originalMethod = AccessTools.Method(grabType, "SpawnPilotWindowLeakFX");
            var prefix = AccessTools.Method(typeof(SilenceCyclopsGrab_Patches), nameof(SilenceCyclopsGrab_Patches.SpawnPilotWindowLeakFX_Prefix));
            harmony.Patch(originalMethod, prefix: new HarmonyMethod(prefix));
        }
        
        bool vehicleFrameworkLoaded = Chainloader.PluginInfos.TryGetValue("com.mikjaw.subnautica.vehicleframework.mod", out var pluginInfo);
        bool correctVersion = false;
        if (pluginInfo != null)
        {
            correctVersion = pluginInfo.Metadata.Version.Major >= 2;
        }
        
        if (vehicleFrameworkLoaded && correctVersion)
        {
            var smallEnoughPrefix = AccessTools.Method(typeof(VehicleFrameworkCompat_Patches),
                nameof(VehicleFrameworkCompat_Patches.IsThisVehicleSmallEnough_Prefix));
            
            var handleDockedPrefix = AccessTools.Method(typeof(VehicleFrameworkCompat_Patches),
                nameof(VehicleFrameworkCompat_Patches.HandleMVDocked_Prefix));
            
            var dockedPositionTranspiler = AccessTools.Method(typeof(VehicleFrameworkCompat_Patches),
                nameof(VehicleFrameworkCompat_Patches.UpdateDockedPosition_Transpiler));
            
            var patchMethodType = Type.GetType("VehicleFramework.Patches.VehicleDockingBayPatch, VehicleFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            
            var smallEnoughMethod = AccessTools.Method(patchMethodType, "IsThisVehicleSmallEnough");
            var handleDockedMethod = AccessTools.Method(patchMethodType, "HandleMVDocked");
            var dockedPositionMethod = AccessTools.Method(patchMethodType, "UpdateDockedPositionPrefix");
            
            harmony.Patch(smallEnoughMethod, prefix: new HarmonyMethod(smallEnoughPrefix));
            harmony.Patch(handleDockedMethod, prefix: new HarmonyMethod(handleDockedPrefix));
            harmony.Patch(dockedPositionMethod, transpiler: new HarmonyMethod(dockedPositionTranspiler));
        }

        sw.Stop();
        Plugin.Logger.LogInfo($"Dependant patches registered in {sw.ElapsedMilliseconds}ms");
    }
}