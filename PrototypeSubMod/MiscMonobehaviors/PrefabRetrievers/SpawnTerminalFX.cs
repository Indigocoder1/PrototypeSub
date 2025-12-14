using System;
using System.Collections;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.MiscMonobehaviors.PrefabRetrievers;

internal class SpawnTerminalFX : MonoBehaviour
{
    [SerializeField] private PrecursorComputerTerminal terminal;
    [SerializeField] private string[] removeFXPaths;
    
    private void Start()
    {
        CoroutineHost.StartCoroutine(SpawnFX());
    }

    private IEnumerator SpawnFX()
    {
        var prefabRequest = PrefabDatabase.GetPrefabAsync("d200d747-b802-43f4-80b1-5c3d2155fbcd");

        yield return prefabRequest;

        GameObject prefab = null;
        if (!prefabRequest.TryGetPrefab(out prefab)) throw new Exception("Error retrieving alien terminal prefab");

        prefab.SetActive(false);
        GameObject fx = prefab.transform.Find("FX").gameObject;
        fx.transform.localRotation = Quaternion.identity;

        var newFX = Instantiate(fx, transform, false);
        if (terminal)
        {
            terminal.fx = newFX;
            terminal.fxControl = newFX.GetComponent<VFXController>();
            terminal.scaleControl = newFX.GetComponent<VFXLerpScale>();
        }
        
        foreach (var path in removeFXPaths)
        {
            Destroy(newFX.transform.Find(path).gameObject);
        }
    }

    public void SetRemoveFXPaths(params string[] paths)
    {
        removeFXPaths = paths;
    }
}
