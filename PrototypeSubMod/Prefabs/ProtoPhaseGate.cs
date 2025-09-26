using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using PrototypeSubMod.Utility;
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

    private static IEnumerator GetGameObject(IOut<GameObject> prefabOut)
    {
        var prefab = Plugin.AssetBundle.LoadAsset<GameObject>("PhaseGate");
        var instance = GameObject.Instantiate(prefab);

        foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
        {
            collider.gameObject.layer = LayerID.Useable;
        }
        
        yield return ProtoMatDatabase.ReplaceVanillaMats(instance);
        
        MaterialUtils.ApplySNShaders(instance, modifiers: new ProtoMaterialModifier(6));
        prefabOut.Set(instance);
    }
}