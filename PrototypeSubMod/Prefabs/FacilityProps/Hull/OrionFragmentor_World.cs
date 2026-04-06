using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Utility;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.Prefabs.FacilityProps.Hull;

public static class OrionFragmentor_World
{
    public static PrefabInfo prefabInfo;

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoOrionFragmentor", null, null);
        
        var prefab = new CustomPrefab(prefabInfo);
        prefab.SetGameObject(GetPrefab);

        prefab.Register();
    }

    private static readonly List<Type> whitelistedComponents = new()
    {
        typeof(Transform),
        typeof(Renderer),
        typeof(MeshFilter),
        typeof(Collider),
        typeof(ConstructionObstacle)
    };
    
    private static readonly List<EntityData> decoProps2Data = new()
    {
        new EntityData(new Vector3(4.837f, 2.429f, 0.931f), Vector3.one * 0.805f, new Vector3(44.869f, 324.870f, 90)),
        new EntityData(new Vector3(-2.536f, 2.429f, -4.256f), Vector3.one * 0.805f, new Vector3(44.869f, 144.870f, 90.000f)),
        new EntityData(new Vector3(2.818f, 5.262f, -4.045f), Vector3.one * 0.805f, new Vector3(29.893f, 90.091f, 144.648f)),
        new EntityData(new Vector3(0.369f, 5.729f, -0.593f), new Vector3(1.069f, 0.805f, 0.805f), new Vector3(89.909f, 326.782f, 0))
    };

    private static readonly List<EntityData> surgicalMachineData = new()
    {
        new EntityData(new Vector3(1.388f, 2.415f, -1.484f), Vector3.one * 0.677f, new Vector3(46.983f, 144.870f, 90)),
        new EntityData(new Vector3(1.112f, 2.415f, -1.620f), Vector3.one * 0.677f, new Vector3(31.131f, 89.173f, 322.844f)),
        new EntityData(new Vector3(0.894f, 2.415f, -1.833f), Vector3.one * 0.677f, new Vector3(46.983f, 324.685f, 90)),
        new EntityData(new Vector3(-0.521f, 2.786f, 0.722f), Vector3.one, new Vector3(0, 324.348f, 0)),
    };

    private static readonly List<EntityData> cablesMiddleData = new()
    {
        new EntityData(new Vector3(3.497f, 5.882f, -4.833f), Vector3.one * 0.4f, new Vector3(319.858f, 274.487f, 316.748f)),
        new EntityData(new Vector3(3.941f, 6.790f, -5.530f), Vector3.one * 0.4f, new Vector3(331.797f, 328.566f, 276.135f)),
        new EntityData(new Vector3(4.074f, 7.8f, -5.694f), Vector3.one * 0.4f, new Vector3(0, 359.814f, 258.111f)),
        new EntityData(new Vector3(3.348f, 5.715f, -4.599f), Vector3.one * 0.4f, new Vector3(314.276f, 281.451f, 305.293f)),
        new EntityData(new Vector3(3.834f, 6.458f, -5.366f), Vector3.one * 0.4f, new Vector3(325.731f, 328.638f, 277.209f)),
        new EntityData(new Vector3(4.058f, 7.520f, -5.684f), Vector3.one * 0.4f, new Vector3(353.139f, 352.853f, 270.836f)),
        new EntityData(new Vector3(3.664f, 6.116f, -5.098f), Vector3.one * 0.4f, new Vector3(316.308f, 304.156f, 285.570f)),
        new EntityData(new Vector3(3.999f, 7.060f, -5.614f), Vector3.one * 0.4f, new Vector3(333.860f, 335.439f, 265.867f)),
        new EntityData(new Vector3(3.135f, 5.5f, -4.28f), Vector3.one * 0.4f, new Vector3(315.099f, 274.578f, 311.634f))
    };
    
    private static readonly List<EntityData> cablesStartData = new()
    {
        new EntityData(new Vector3(2.947f, 5.282f, -4.028f), Vector3.one * 0.4f, new Vector3(346.741f, 32.793f, 42.875f)),
        new EntityData(new Vector3(4.079f, 7.974f, -5.690f), Vector3.one * 0.4f, new Vector3(0, 359.814f, 270)),
    };
    
    private static IEnumerator GetPrefab(IOut<GameObject> prefabOut)
    {
        var empty = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("Empty");
        var instance = UWE.Utils.InstantiateDeactivated(empty);

        PrefabUtils.AddBasicComponents(instance, prefabInfo.ClassID, prefabInfo.TechType,
            LargeWorldEntity.CellLevel.Far);
        
        var decoProps2Prefab = PrefabDatabase.GetPrefabAsync("3bbf8830-e34f-43a1-bbb3-743c7e6860ac");
        yield return decoProps2Prefab;

        if (!decoProps2Prefab.TryGetPrefab(out var decoProps2)) throw new Exception("Error loading decoProps2 prefab");

        foreach (var data in decoProps2Data)
        {
            var prop = GameObject.Instantiate(decoProps2, instance.transform);
            prop.transform.localPosition = data.position;
            prop.transform.localScale = data.scale;
            prop.transform.localEulerAngles = data.eulerAngles;
            DisplayCaseProp.TrimComponents(prop, whitelistedComponents);
        }

        var surgicalMachinePrefab = PrefabDatabase.GetPrefabAsync("6a01a336-fb46-469a-9f7d-1659e07d11d7");
        yield return surgicalMachinePrefab;

        if (!surgicalMachinePrefab.TryGetPrefab(out var surgicalMachine)) throw new Exception("Error loading surgicalMachine prefab");
        foreach (var data in surgicalMachineData)
        {
            var prop = GameObject.Instantiate(surgicalMachine, instance.transform);
            prop.transform.localPosition = data.position;
            prop.transform.localScale = data.scale;
            prop.transform.localEulerAngles = data.eulerAngles;
            DisplayCaseProp.TrimComponents(prop, whitelistedComponents);
        }

        var cablesMiddle1Prefab = PrefabDatabase.GetPrefabAsync("69cd7462-7cd2-456c-bfff-50903c391737");
        yield return cablesMiddle1Prefab;
        
        if (!cablesMiddle1Prefab.TryGetPrefab(out var cablesMiddle1)) throw new Exception("Error loading cablesMiddle1 prefab");
        foreach (var data in cablesMiddleData)
        {
            var prop = GameObject.Instantiate(cablesMiddle1, instance.transform);
            prop.transform.localPosition = data.position;
            prop.transform.localScale = data.scale;
            prop.transform.localEulerAngles = data.eulerAngles;
            DisplayCaseProp.TrimComponents(prop, whitelistedComponents);
        }
        
        var cablesStart1Prefab = PrefabDatabase.GetPrefabAsync("18aa16f9-d1d8-4ccd-8a10-7ad32a5fd283");
        yield return cablesStart1Prefab;
        
        if (!cablesStart1Prefab.TryGetPrefab(out var cablesStart1)) throw new Exception("Error loading cablesStart1 prefab");
        foreach (var data in cablesStartData)
        {
            var prop = GameObject.Instantiate(cablesStart1, instance.transform);
            prop.transform.localPosition = data.position;
            prop.transform.localScale = data.scale;
            prop.transform.localEulerAngles = data.eulerAngles;
            DisplayCaseProp.TrimComponents(prop, whitelistedComponents);
        }
        
        var labTablePrefab = PrefabDatabase.GetPrefabAsync("df9aed66-c131-4570-9dcd-1e3d2109dcaa");
        yield return labTablePrefab;
        
        if (!labTablePrefab.TryGetPrefab(out var labTable)) throw new Exception("Error loading labTable prefab");
        
        var table = GameObject.Instantiate(labTable, instance.transform);
        table.transform.localPosition = Vector3.zero;
        table.transform.localScale = Vector3.one;
        table.transform.localEulerAngles = new Vector3(0, 324.568f, 0);
        DisplayCaseProp.TrimComponents(table, whitelistedComponents);
        
        var teleporterPrefab = PrefabDatabase.GetPrefabAsync("9a5ff289-4b82-4b9c-b9f6-d60f46dd2d7a");
        yield return teleporterPrefab;
        
        if (!teleporterPrefab.TryGetPrefab(out var teleporter)) throw new Exception("Error loading teleporter prefab");
        
        var teleporterInstance = GameObject.Instantiate(teleporter, instance.transform);
        teleporterInstance.transform.localPosition = new Vector3(1.167f, -3.328f, -1.717f);
        teleporterInstance.transform.localScale = Vector3.one * 0.837f;
        teleporterInstance.transform.localEulerAngles = new Vector3(0, 324.999f, 0);
        DisplayCaseProp.TrimComponents(teleporterInstance, whitelistedComponents);
        
        var controlPillarPrefab = PrefabDatabase.GetPrefabAsync("f5da7019-e0d4-4f5f-ba54-27b8fe3dd21e");
        yield return controlPillarPrefab;
        
        if (!controlPillarPrefab.TryGetPrefab(out var controlPillar)) throw new Exception("Error loading controlPillar prefab");
        
        var pillar = GameObject.Instantiate(controlPillar, instance.transform);
        pillar.transform.localPosition = new Vector3(1.177f, 6.168f, -1.737f);
        pillar.transform.localScale = new Vector3(1.253f, 2.071f, 1.253f);
        pillar.transform.localEulerAngles = new Vector3(0, 143.377f, 180);
        DisplayCaseProp.TrimComponents(teleporterInstance, whitelistedComponents);

        prefabOut.Set(instance);
    }

    private struct EntityData
    {
        public Vector3 position;
        public Vector3 scale;
        public Vector3 eulerAngles;

        public EntityData(Vector3 position, Vector3 scale, Vector3 eulerAngles)
        {
            this.position = position;
            this.scale = scale;
            this.eulerAngles = eulerAngles;
        }
    }
}