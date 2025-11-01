using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public static class ProtoTransmissionDevice
{
    public static PrefabInfo prefabInfo;

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoTransmissionDevice",null, null, unlockAtStart: true);

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetGameObject);
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("ProtoTransmissionDevice.json"));
        prefab.SetEquipment(Plugin.PhaseGateEquipmentType);
        
        prefab.Register();
    }
    
    private static GameObject GetGameObject()
    {
        var asset = Plugin.AssetBundle.LoadAsset<GameObject>("ProtoTransmissionDevice");
        asset.gameObject.SetActive(false);
        var instance = GameObject.Instantiate(asset);

        MaterialUtils.ApplySNShaders(instance, modifiers: new ProtoMaterialModifier(6f));

        return instance;
    }
}