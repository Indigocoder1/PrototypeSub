using System;
using System.Reflection;
using HarmonyLib;

namespace PrototypeSubMod.Patches;

public static class CyclopsModulesCompat_Patches
{
    public static Type CyclopsModulesComponentType
    {
        get
        {
            _cyclopsModulesComponentType ??= Type.GetType(
                "CyclopsModules.MonoBehaviours.CyclopsModulesComponent, CyclopsModules, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            );

            return _cyclopsModulesComponentType;
        }
    }
    
    private static Type _cyclopsModulesComponentType;

    private static FieldInfo AccelMultiplierFieldInfo
    {
        get
        {
            _accelMultiplierFieldInfo ??= CyclopsModulesComponentType.GetField("accelMultiplier", AccessTools.all);

            return _accelMultiplierFieldInfo;
        }
    }

    private static FieldInfo _accelMultiplierFieldInfo;
    
    private static FieldInfo DefenseMultiplierFieldInfo
    {
        get
        {
            _defenseMultiplierFieldInfo ??= CyclopsModulesComponentType.GetField("defenseMultiplier", AccessTools.all);

            return _defenseMultiplierFieldInfo;
        }
    }

    private static FieldInfo _defenseMultiplierFieldInfo;
    
    public static void CyclopsModulesComponentStart_Postfix(object __instance)
    {
        DefenseMultiplierFieldInfo.SetValue(__instance, 1);
        AccelMultiplierFieldInfo.SetValue(__instance, 1);
    }
}