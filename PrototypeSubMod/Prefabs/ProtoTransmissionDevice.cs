using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Utility;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public static class ProtoTransmissionDevice
{
    public static PrefabInfo prefabInfo;

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoTransmissionDevice", null, null)
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("TransmissionDevice_Icon"))
            .WithSizeInInventory(new Vector2int(2, 2));

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetGameObject);
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("ProtoTransmissionDevice.json"));
        prefab.SetEquipment(Plugin.PhaseGateEquipmentType);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);

        CraftDataHandler.SetBackgroundType(prefabInfo.TechType, CraftData.BackgroundType.Blueprint);

        prefab.Register();
    }
    
    private static GameObject GetGameObject()
    {
        var asset = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("ProtoTransmissionDevice");
        asset.gameObject.SetActive(false);
        var instance = GameObject.Instantiate(asset);

        MaterialUtils.ApplySNShaders(instance, modifiers: new ProtoMaterialModifier(6f));

        return instance;
    }
}