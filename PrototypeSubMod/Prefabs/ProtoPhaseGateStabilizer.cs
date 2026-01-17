using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using PrototypeSubMod.Compatibility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public class ProtoPhaseGateStabilizer
{
    public static PrefabInfo PrefabInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("ProtoPhaseGateStabilizer", null, null)
            .WithSizeInInventory(new Vector2int(2, 2))
            .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("ProtoPhaseGateStabilizer_Icon"));

        var prefab = new CustomPrefab(PrefabInfo);
        
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("ProtoPhaseGateStabilizer.json"))
            .WithCraftingTime(10f);
        prefab.SetGameObject(GetGameObject);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);
        prefab.SetUnlock(ProtoPhaseGate.PrefabInfo.TechType);

        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var prefab = Plugin.AssetBundle.LoadAsset<GameObject>("ProtoPhaseGateStabilizer");
        var instance = GameObject.Instantiate(prefab);
        
        return instance;
    }
}