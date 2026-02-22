using System;
using PrototypeSubMod.Misc;
using PrototypeSubMod.Prefabs;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.PrefabRetrievers;

public class ElectricFireSpawner : PrefabSpawnBase
{
    public override bool HasValidPrefab()
    {
        return true;
    }

    public override SpawnRequest SpawnObjInternal(Transform objParent)
    {
        return new AsyncSpawnRequest(ProtoElectricFire.PrefabInfo.TechType, objParent);
    }

    private void OnDestroy()
    {
        Plugin.Logger.LogInfo(new System.Diagnostics.StackTrace());
    }
}