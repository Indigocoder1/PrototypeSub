using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Docking;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using UnityEngine;

namespace PrototypeSubMod.Patches;

public class VehicleFrameworkCompat_Patches
{
    public static Type ModVehicleType
    {
        get
        {
            if (_modVehicleType == null)
            {
                _modVehicleType = Type.GetType("VehicleFramework.ModVehicle, VehicleFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            }

            return _modVehicleType;
        }
    }
    
    private static Type _modVehicleType;
    private static MethodInfo _getBoundingDimensions;

    public static MethodInfo OnDockedMethod
    {
        get
        {
            if (_onDockedMethod == null)
            {
                _onDockedMethod = ModVehicleType.GetMethod("OnVehicleDocked", AccessTools.all);
            }

            return _onDockedMethod;
        }
    }
    
    public static MethodInfo OnUndockedMethod
    {
        get
        {
            if (_onUndockedMethod == null)
            {
                _onUndockedMethod = ModVehicleType.GetMethod("OnVehicleUndocked", AccessTools.all);
            }

            return _onUndockedMethod;
        }
    }
    
    private static MethodInfo _onDockedMethod;
    private static MethodInfo _onUndockedMethod;

    public static MethodInfo PlayerExitMethod
    {
        get
        {
            if (_playerExitMethod == null)
            {
                _playerExitMethod = ModVehicleType.GetMethod("PlayerExit", AccessTools.all);
            }

            return _playerExitMethod;
        }
    }

    private static MethodInfo _playerExitMethod;
    
    public static bool IsThisVehicleSmallEnough_Prefix(VehicleDockingBay bay, GameObject nearby, ref bool __result)
    {
        if (_getBoundingDimensions == null)
        {
            _getBoundingDimensions = ModVehicleType.GetMethod("GetBoundingDimensions", AccessTools.all);
        }
        
        var dockingBounds = bay.GetComponent<DockingBayBounds>();
        if (!dockingBounds) return true;

        var modVehicle = GetComponentInHierarchy(nearby, ModVehicleType);
        if (modVehicle == null) return true;

        var checkDimensions = dockingBounds.GetBounds();
        Vector3 boundingDimensions = (Vector3)_getBoundingDimensions.Invoke(modVehicle, null);

        __result = BoundsFit(boundingDimensions, checkDimensions);
        return false;
    }

    public static bool HandleMVDocked_Prefix(Vehicle vehicle, VehicleDockingBay dock)
    {
        if (!VehicleFrameworkCompatManager.IsModdedVehicle(vehicle)) return true;
        
        var dockingBounds = dock.GetComponent<DockingBayBounds>();
        if (!dockingBounds) return true;
        
        Vector3 boundingDimensions = (Vector3)_getBoundingDimensions.Invoke(vehicle, null);
        
        return !BoundsFit(boundingDimensions, dockingBounds.GetBounds());
    }

    public static void MaybeStartCinematicMode_Postfix(PlayerCinematicController cinematic, Player player)
    {
        if (!cinematic.TryGetComponent(out IgnoreCinematicStart ignoreStart)) return;
        
        if (!ignoreStart.enabled && !cinematic.cinematicModeActive)
        {
            cinematic.StartCinematicMode(player);
        }
    }
    
    public static IEnumerable<CodeInstruction> UpdateDockedPosition_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var match = new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "Warn");

        var matcher = new CodeMatcher(instructions)
            .MatchForward(false, match)
            .RemoveInstruction()
            .Advance(-1)
            .RemoveInstruction();
        
        return matcher.InstructionEnumeration();
    }

    private static bool BoundsFit(Vector3 checkFor, Vector3 checkAgainst)
    {
        return checkFor.x <= checkAgainst.x && 
               checkFor.y <= checkAgainst.y &&
               checkFor.z <= checkAgainst.z;
    }

    private static object GetComponentInHierarchy(GameObject obj, Type type)
    {
        var t = obj.GetComponent(type);
        if (t) return t;

        var tParent = obj.GetComponentInParent(type);
        if (tParent) return tParent;
        
        var tChild = obj.GetComponentInChildren(type);
        return tChild;
    }
}