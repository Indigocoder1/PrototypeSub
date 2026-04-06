using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using PrototypeSubMod.MiscMonobehaviors.PrefabRetrievers;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public class ProtoTerminalVFX
{
    public static PrefabInfo prefabInfo;
    
    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoTerminalVFX", null, null);

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetPrefab);

        prefab.Register();
    }

    private static GameObject GetPrefab()
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("Empty");
        prefab.SetActive(false);

        var instance = UWE.Utils.InstantiateDeactivated(prefab);
        PrefabUtils.AddBasicComponents(instance, prefabInfo.ClassID, prefabInfo.TechType,
            LargeWorldEntity.CellLevel.Near);

        var spawnerObj = new GameObject();
        spawnerObj.transform.SetParent(instance.transform, false);
        spawnerObj.transform.position = new Vector3(0, -1.5f, -0.45f);
        var spawner = spawnerObj.EnsureComponent<SpawnTerminalFX>();
        spawner.SetRemoveFXPaths(new[]
        {
            "x_Precursor_ComputerTerminal_SmallSymbol",
            "x_Precursor_ComputerTerminal_Symbol"
        });

        return instance;
    }
}