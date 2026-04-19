using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.DecorativeWyrms;

internal static class ProtoGrandReefWyrms
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoDecorativeWorm", null, null);

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetGameObject);
        PDAHandler.AddCustomScannerEntry(prefabInfo.TechType, prefabInfo.TechType, false,
            1, 5, false);

        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        return Plugin.GeneralAssetBundle.LoadAsset<GameObject>("Empty");
    }
}