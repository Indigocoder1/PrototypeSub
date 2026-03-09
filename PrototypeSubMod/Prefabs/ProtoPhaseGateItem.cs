using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;
using PrototypeSubMod.Compatibility;
using UnityEngine;
using static GameObjectPoolPrefabMap;

namespace PrototypeSubMod.Prefabs;

public class ProtoPhaseGateItem
{
    public static PrefabInfo PrefabInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("ProtoPhaseGateItem", null, null)
            .WithSizeInInventory(new Vector2int(4, 3))
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("ProtoPhaseGate_Icon.png"));

        var prefab = new CustomPrefab(PrefabInfo);
        
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("ProtoPhaseGateItem.json"))
            .WithCraftingTime(10f);
        prefab.SetGameObject(GetGameObject);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);
        prefab.SetEquipment(Plugin.PhaseGateEquipmentType);
        prefab.SetUnlock(ProtoPhaseGate.PrefabInfo.TechType);

        CraftDataHandler.SetBackgroundType(PrefabInfo.TechType, CraftData.BackgroundType.Blueprint);

        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("AlienFramework");
        var instance = GameObject.Instantiate(prefab);

        MaterialUtils.ApplySNShaders(instance);
        
        foreach (var rend in instance.GetComponentsInChildren<Renderer>(true))
        {
            rend.material.color = Color.cyan;
        }
        
        return instance;
    }
}