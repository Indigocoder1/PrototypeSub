using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace PrototypeSubMod.Utility;

internal static class SetupSaveStateReferences
{
    private static Dictionary<FieldInfo, object> SaveStateReferences = new();
    private static bool eventAdded;

    public static void SetupReferences(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic);
            fields.AddRangeToArray(type.GetFields(BindingFlags.Static | BindingFlags.Public));
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute is SaveStateReferenceAttribute reference)
                    {
                        SaveStateReferences.Add(field, reference.defaultValue);
                        break;
                    }
                }
            }
        }

        if (!eventAdded)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            eventAdded = true;
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main") return;
        Plugin.Logger.LogInfo($"Resetting save state references");

        foreach (var info in SaveStateReferences.Keys)
        {
            info.SetValue(null, SaveStateReferences[info]);
        }
    }
}
