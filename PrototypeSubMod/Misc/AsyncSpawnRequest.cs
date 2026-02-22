using System;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Misc;

public class AsyncSpawnRequest : SpawnRequest
{
    private event Action<SpawnRequest> spawnCallbacks;

    private Coroutine spawnCoroutine;
    
    public override void RegisterCallback(Action<SpawnRequest> spawnCallback)
    {
        spawnCallbacks += spawnCallback;
    }

    public override void Release()
    {
        UWE.CoroutineHost.StopCoroutine(spawnCoroutine);
    }

    public AsyncSpawnRequest(TechType techType, Transform parent)
    {
        spawnCoroutine = UWE.CoroutineHost.StartCoroutine(SpawnPrefab(techType, parent));
    }

    private IEnumerator SpawnPrefab(TechType techType, Transform parent)
    {
        var task = CraftData.GetPrefabForTechTypeAsync(techType);
        yield return task;

        var prefab = task.GetResult();
        Result = GameObject.Instantiate(prefab, parent);
        spawnCallbacks?.Invoke(this);
    }
}