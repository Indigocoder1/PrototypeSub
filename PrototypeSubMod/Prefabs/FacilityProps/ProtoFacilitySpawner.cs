using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.FacilityProps;

public static class ProtoFacilitySpawner
{
    public static void Register(string classID, string prefabName)
    {
        var prefabInfo = PrefabInfo.WithTechType(classID,null, null);

        var prefab = new CustomPrefab(prefabInfo);

        var gameObject = GetGameObject(prefabName);
        prefab.SetGameObject(gameObject);
        prefab.SetSpawns(new SpawnLocation(Vector3.zero));
        
        prefab.Register();
    }

    private static GameObject GetGameObject(string prefabName)
    {
        var asset = Plugin.GeneralAssetBundle.LoadAsset<GameObject>(prefabName);
        asset.gameObject.SetActive(false);
        return asset;
    }
}