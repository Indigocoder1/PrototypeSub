using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using PrototypeSubMod.Compatibility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public class ProtoPhaseGateDeterrent
{
    public static PrefabInfo PrefabInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("ProtoPhaseGateDeterrent", null, null)
            .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("exosuitgrapplingarmmodule_Upgraded"));

        var prefab = new CustomPrefab(PrefabInfo);
        
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("ProtoPhaseGateDeterrent.json"))
            .WithCraftingTime(10f);
        prefab.SetGameObject(GetGameObject);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);

        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var prefab = Plugin.AssetBundle.LoadAsset<GameObject>("AlienFramework");
        var instance = GameObject.Instantiate(prefab);

        foreach (var rend in instance.GetComponentsInChildren<Renderer>(true))
        {
            rend.material.color = Color.cyan;
        }
        
        return instance;
    }
}