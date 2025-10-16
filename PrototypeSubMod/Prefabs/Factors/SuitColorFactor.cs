using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using PrototypeSubMod.Factors;

namespace PrototypeSubMod.Prefabs.Factors;

public static class SuitColorFactor
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("SuitColorFactor", unlockAtStart: true).WithIcon(SpriteManager.Get(TechType.ComputerChip));

        var prefab = new CustomPrefab(prefabInfo);

        var cloneTemplate = new CloneTemplate(prefabInfo, TechType.Compass);
        cloneTemplate.ModifyPrefab += gameObject =>
        {
            gameObject.name = "SuitColorFactor";
            gameObject.GetComponent<PrefabIdentifier>().classId = prefabInfo.ClassID;
            gameObject.AddComponent<ColorFactor>();
        };

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
        prefab.SetGameObject(cloneTemplate);
        prefab.Register();
    }
}