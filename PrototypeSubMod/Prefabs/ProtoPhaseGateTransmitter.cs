using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using PrototypeSubMod.Compatibility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public class ProtoPhaseGateTransmitter
{
    public static PrefabInfo PrefabInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("ProtoPhaseGateTransmitter", null, null)
            .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("exosuitgrapplingarmmodule_Upgraded"));

        var prefab = new CustomPrefab(PrefabInfo);
        
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("ProtoPhaseGateTransmitter.json"))
            .WithCraftingTime(10f);
        prefab.SetGameObject(GetGameObject);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);

        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var prefab = Plugin.AssetBundle.LoadAsset<GameObject>("ProtoPhaseGateTransmitter");
        var instance = GameObject.Instantiate(prefab);
        
        return instance;
    }
}