using Nautilus.Assets;
using Nautilus.Utility;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.FacilityProps;

internal class ProtoLogo_World
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("WorldProtoLogo", null, null, "English");

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetPrefab);

        prefab.Register();
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefabOut)
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("ProtoLogo");
        prefab.SetActive(false);

        var gameObject = GameObject.Instantiate(prefab, new Vector3(0, 500f, 0), Quaternion.identity);

        yield return new WaitUntil(() => MaterialUtils.IsReady);

        MaterialUtils.ApplySNShaders(gameObject);

        prefabOut.Set(gameObject);
    }
}
