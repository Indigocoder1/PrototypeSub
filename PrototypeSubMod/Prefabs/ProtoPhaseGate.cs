using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public static class ProtoPhaseGate
{
    public static PrefabInfo PrefabInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("ProtoPhaseGate", null, null);

        var prefab = new CustomPrefab(PrefabInfo);
        
        prefab.SetGameObject(GetGameObject);

        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var prefab = Plugin.AssetBundle.LoadAsset<GameObject>("PhaseGate");
        var instance = GameObject.Instantiate(prefab);
        MaterialUtils.ApplySNShaders(instance);
        
        return instance;
    }
}