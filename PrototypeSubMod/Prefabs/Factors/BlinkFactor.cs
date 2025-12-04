using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Utility;
using PrototypeSubMod.Factors.Blink;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.Factors;

public static class BlinkFactor
{
    public static PrefabInfo prefabInfo { get; private set; }

    private static CustomPrefab prefab;

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("BlinkFactor", unlockAtStart: true)
            .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("ProtoFactorIcon"));

        prefab = new CustomPrefab(prefabInfo);

        prefab.SetRecipe(new RecipeData
        {
            craftAmount = 1,
            Ingredients = new List<Ingredient>
            {
                new(TechType.Titanium, 1)
            }
        }).WithCraftingTime(3f);
        prefab.SetEquipment(Plugin.FactorEquipmentType);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }
    
    private static IEnumerator GetPrefab(IOut<GameObject> prefabOut)
    {
        var prefab = Plugin.AssetBundle.LoadAsset<GameObject>("ProtoBlinkFactor");
        prefab.SetActive(false);

        var instance = GameObject.Instantiate(prefab);

        yield return new WaitUntil(() => MaterialUtils.IsReady);

        MaterialUtils.ApplySNShaders(instance, modifiers: new ProtoMaterialModifier(3, 0));

        yield return ProtoMatDatabase.ReplaceVanillaMats(instance);
        
        prefabOut.Set(instance);
    }
}