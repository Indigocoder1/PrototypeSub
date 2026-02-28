using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using PrototypeSubMod.MiscMonobehaviors;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.FacilityProps;

internal class CustomPing
{
    private static List<Type> WhitelistedComponents = new()
    {
        typeof(Transform),
        typeof(PrefabIdentifier),
        typeof(TechTag),
        typeof(LargeWorldEntity),
        typeof(PingInstance)
    };

    public static TechType CreatePing(string techType, PingType pingType, Color colorOverride = default, float minDist = 25, 
        float fadeRange = 10, Vector3[] spawnPositions = null, bool displayInManager = true, params Type[] components)
    {
        var prefabInfo = PrefabInfo.WithTechType(techType);

        var prefab = new CustomPrefab(prefabInfo);
        var cloneTemplate = new CloneTemplate(prefabInfo, TechType.Beacon);
        cloneTemplate.ModifyPrefab += gameObject =>
        {
            SetupGameObject(ref gameObject, prefabInfo, pingType, colorOverride, minDist, fadeRange, displayInManager, components);
        };

        prefab.SetGameObject(cloneTemplate);

        if (spawnPositions != null)
        {
            SpawnLocation[] locations = new SpawnLocation[spawnPositions.Length];
            for (int i = 0; i < spawnPositions.Length; i++)
            {
                locations[i] = new SpawnLocation(spawnPositions[i]);
            }
            prefab.SetSpawns(locations);
        }

        prefab.Register();
        return prefabInfo.TechType;
    }

    private static void SetupGameObject(ref GameObject beacon, PrefabInfo info, PingType pingType, Color colorOverride, 
        float minDist, float fadeRange, bool displayInManager, Type[] components)
    {
        beacon.name = info.TechType.ToString();

        DisplayCaseProp.TrimComponents(beacon, WhitelistedComponents);

        foreach (Transform child in beacon.transform)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }

        foreach (var type in components)
        {
            beacon.AddComponent(type);
        }

        var pingInstance = beacon.EnsureComponent<PingInstance>();
        pingInstance.pingType = pingType;
        pingInstance.origin = beacon.transform;
        pingInstance.displayPingInManager = displayInManager;
        pingInstance.minDist = minDist;
        pingInstance.range = fadeRange;
        pingInstance.visitable = true;
        pingInstance.visitDistance = 100;
        pingInstance.visitDuration = 2f;
        pingInstance.SetColor(0);

        var pingSetter = beacon.EnsureComponent<DelayedPingLabelSetter>();

        pingSetter.translationKey = info.TechType.ToString();
        pingSetter.pingInstance = pingInstance;

        var signalPing = beacon.EnsureComponent<SignalPing>();
        signalPing.pingInstance = pingInstance;
        signalPing.disableOnEnter = true;
        signalPing.descriptionKey = info.TechType.ToString();

        var col = beacon.EnsureComponent<SphereCollider>();
        col.radius = 10;
        col.isTrigger = true;

        if (colorOverride != default)
        {
            beacon.EnsureComponent<PingColorOverride>().overrideColor = colorOverride;
        }
    }
}
