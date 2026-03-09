using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Factors.Tether;
using PrototypeSubMod.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.Factors;

public static class TetherFactor
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("TetherFactor", unlockAtStart: false)
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("ProtoFactorIcon"));

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("TetherFactor.json"))
            .WithCraftingTime(3f);
        prefab.SetEquipment(Plugin.FactorEquipmentType);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);
        prefab.SetGameObject(GetPrefab);
        CraftDataHandler.SetBackgroundType(prefabInfo.TechType, CraftData.BackgroundType.PlantAirSeed);
        prefab.Register();
    }
    
    private static IEnumerator GetPrefab(IOut<GameObject> prefabOut)
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("ProtoTetherFactor");
        prefab.SetActive(false);

        var instance = GameObject.Instantiate(prefab);

        yield return new WaitUntil(() => MaterialUtils.IsReady);

        MaterialUtils.ApplySNShaders(instance, modifiers: new ProtoMaterialModifier(3, 0));

        yield return ProtoMatDatabase.ReplaceVanillaMats(instance);
        
        prefabOut.Set(instance);
    }
}