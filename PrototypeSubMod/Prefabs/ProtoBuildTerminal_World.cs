using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using PrototypeSubMod.Utility;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class ProtoBuildTerminal_World
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        var info = PrefabInfo.WithTechType("ProtoBuildTerminal");

        prefabInfo = info;

        var prefab = new CustomPrefab(info);

        prefab.SetGameObject(GetPrefab);

        prefab.SetSpawns(new SpawnLocation[]
        {
            new(new Vector3(466.08f, -93.86f, 1178.77f), new Vector3(0f, 110f, 0f))
        });

        prefab.Register();
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefabOut)
    {
        GameObject model = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("ProtoBuildTerminal");
        model.SetActive(false);

        var instantiatedPrefab = GameObject.Instantiate(model);

        yield return new WaitUntil(() => MaterialUtils.IsReady);

        MaterialUtils.ApplySNShaders(instantiatedPrefab, modifiers: new ProtoMaterialModifier(30, 0));
        
        yield return ProtoMatDatabase.ReplaceVanillaMats(instantiatedPrefab);

        prefabOut.Set(instantiatedPrefab);
    }

}
