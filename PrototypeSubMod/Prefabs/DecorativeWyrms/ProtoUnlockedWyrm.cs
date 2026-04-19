using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.DecorativeWyrms;

internal static class ProtoUnlockedWyrm
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoUnlockedWorm", null, null);

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetGameObject);

        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        return Plugin.GeneralAssetBundle.LoadAsset<GameObject>("Empty");
    }
}