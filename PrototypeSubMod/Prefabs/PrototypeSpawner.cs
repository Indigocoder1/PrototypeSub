using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public class PrototypeSpawner
{
    public static PrefabInfo prefabInfo;
    
    public static void Register()
    {
        var prefabInfo = PrefabInfo.WithTechType("ProtoSubSpawner",null, null, "English");

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetGameObject);
        prefab.SetSpawns(new SpawnLocation(Vector3.zero));
        
        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var asset = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("PrototypeSpawner");
        asset.gameObject.SetActive(false);
        return UWE.Utils.InstantiateDeactivated(asset);
    }
}