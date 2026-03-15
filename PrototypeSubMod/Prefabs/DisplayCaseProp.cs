using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using PrototypeSubMod.MiscMonobehaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.Prefabs;

internal static class DisplayCaseProp
{
    public static void Register(string displayObjectClassID, string newTechType, TechType scanTechType, Vector3 localOffset, Vector3 localScale, string[] destroyChildObjects = null)
    {
        var prefabInfo = PrefabInfo.WithTechType(newTechType, null, null, "English");

        var prefab = new CustomPrefab(prefabInfo);
        var cloneTemplate = new CloneTemplate(prefabInfo, "d0fea4da-39f2-47b4-aece-bb12fe7f9410");

        CraftData.PreparePrefabIDCache();
        cloneTemplate.ModifyPrefabAsync = gameObject => ModifyPrefab(gameObject, displayObjectClassID, scanTechType,
            localOffset, localScale, destroyChildObjects);

        prefab.SetGameObject(cloneTemplate);

        prefab.Register();
    }

    private static IEnumerator ModifyPrefab(GameObject prefab, string displayObjectClassID, TechType scanTechType, Vector3 localOffset, Vector3 localScale, string[] destroyChildObjects)
    {
        var prefabTask = PrefabDatabase.GetPrefabAsync(displayObjectClassID);
        yield return prefabTask;

        if (!prefabTask.TryGetPrefab(out var modelPrefab))
        {
            throw new Exception($"Error retrieving prefab with class ID = {displayObjectClassID}");
        }
        modelPrefab.gameObject.SetActive(false);
        var instance = GameObject.Instantiate(modelPrefab, prefab.transform);
        if (destroyChildObjects != null)
        {
            foreach (var path in destroyChildObjects)
            {
                GameObject.DestroyImmediate(instance.transform.Find(path).gameObject);
            }
        }

        TrimComponents(instance, whitelistedComponents);

        instance.SetActive(true);

        modelPrefab.gameObject.SetActive(true);

        if (prefab.TryGetComponent(out SkyApplier applier))
        {
            applier.renderers.AddRangeToArray(instance.GetComponentsInChildren<Renderer>());
        }

        var col = instance.AddComponent<CapsuleCollider>();
        instance.transform.localPosition = localOffset;
        instance.transform.localScale = localScale;
        col.radius = 1f / instance.transform.lossyScale.x;
        col.height = 6.3f / instance.transform.lossyScale.x;

        var tag = instance.AddComponent<TechTag>();
        tag.type = scanTechType;

        instance.AddComponent<MarkAsEntityRoot>();
    }

    public static readonly List<Type> whitelistedComponents = new()
    {
        typeof(Transform),
        typeof(Renderer),
        typeof(MeshFilter),
    };

    public static void TrimComponents(GameObject instance, List<Type> whitelistedComponents)
    {
        var components = instance.GetComponentsInChildren<Component>(true);
        var ignoreFirstPass = new List<Type>();

        foreach (var component in components)
        {
            var type = component.GetType();
            var attributes = type.GetCustomAttributes(true);
            foreach (var attribute in attributes)
            {
                if (attribute is RequireComponent require)
                {
                    if (require.m_Type0 != null && !whitelistedComponents.Contains(require.m_Type0)) ignoreFirstPass.Add(require.m_Type0);
                    if (require.m_Type1 != null && !whitelistedComponents.Contains(require.m_Type1)) ignoreFirstPass.Add(require.m_Type1);
                    if (require.m_Type2 != null && !whitelistedComponents.Contains(require.m_Type2)) ignoreFirstPass.Add(require.m_Type2);
                }
            }
        }

        ignoreFirstPass.AddRange(whitelistedComponents);
        TrimComponents(components, ignoreFirstPass);
        TrimComponents(components, whitelistedComponents);
    }

    private static void TrimComponents(Component[] components, List<Type> ignore)
    {
        foreach (var component in components)
        {
            var type = component.GetType();
            if (ignore.Any(t => type.IsSubclassOf(t) || type == t)) continue;

            GameObject.DestroyImmediate(component);
        }
    }
}
