using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public static class ProtoAggressiveWyrm
{
    public static PrefabInfo prefabInfo;
    
    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoAggressiveWyrm", null, null);

        var prefab = new CustomPrefab(prefabInfo);
        
        prefab.SetGameObject(GetGameObject);

        prefab.Register();
    }
    
    private static IEnumerator GetGameObject(IOut<GameObject> prefabOut)
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("ProtoAgressiveWorm");
        var instance = GameObject.Instantiate(prefab);
        
        yield return ProtoMatDatabase.ReplaceVanillaMats(instance);
        
        MaterialUtils.ApplySNShaders(instance, modifiers: new ProtoMaterialModifier(6));
        prefabOut.Set(instance);
    }
}