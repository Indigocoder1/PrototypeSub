using System.Collections;
using Nautilus.Assets;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.Factors;

public static class LocatorFactorSource
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("LocatorFactorSource");

        var prefab = new CustomPrefab(prefabInfo);
        
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }
    
    private static GameObject GetPrefab()
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("LocatorFactorSource");
        prefab.SetActive(false);

        var instance = UWE.Utils.InstantiateDeactivated(prefab);
        return instance;
    }
}