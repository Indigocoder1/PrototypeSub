using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.DecorativeWyrms;

internal static class ProtoGrassyWyrms
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoGrassyWyrm", null, null);

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetGameObject);
        PDAHandler.AddCustomScannerEntry(prefabInfo.TechType, prefabInfo.TechType, false,
            1, 5, false);

        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("ProtoGrassyWyrm");
        var instance = UWE.Utils.InstantiateDeactivated(prefab);
        MaterialUtils.ApplySNShaders(instance);

        return instance;
    }
}