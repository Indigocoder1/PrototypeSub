using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;
using PrototypeSubMod.Compatibility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public class ProtoPhaseGateStructure
{
    public static PrefabInfo PrefabInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("ProtoPhaseGateStructure", null, null)
            .WithSizeInInventory(new Vector2int(2, 2))
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("ProtoPhaseGateStructure_Icon"));

        var prefab = new CustomPrefab(PrefabInfo);
        
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("ProtoPhaseGateStructure.json"))
            .WithCraftingTime(10f);
        prefab.SetGameObject(GetGameObject);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);
        prefab.SetUnlock(ProtoPhaseGate.PrefabInfo.TechType);

        CraftDataHandler.SetBackgroundType(PrefabInfo.TechType, CraftData.BackgroundType.ExosuitArm);

        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("ProtoPhaseGateStructure");
        var instance = GameObject.Instantiate(prefab);

        MaterialUtils.ApplySNShaders(instance);
        
        return instance;
    }
}