using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Factors;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.Factors;

public static class SuitColorFactor
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("SuitColorFactor")
            .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("ProtoFactorIcon"));

        var prefab = new CustomPrefab(prefabInfo);

        var cloneTemplate = new CloneTemplate(prefabInfo, TechType.Compass);
        cloneTemplate.ModifyPrefab += gameObject =>
        {
            gameObject.name = "SuitColorFactor";
            gameObject.GetComponent<PrefabIdentifier>().classId = prefabInfo.ClassID;
            gameObject.AddComponent<ColorFactor>();
        };

        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("ColorFactor.json"))
            .WithCraftingTime(3f);
        prefab.SetEquipment(Plugin.FactorEquipmentType);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);
        prefab.SetGameObject(cloneTemplate);
        prefab.Register();
    }
}