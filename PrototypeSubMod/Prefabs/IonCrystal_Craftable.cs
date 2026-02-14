using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using PrototypeSubMod.Compatibility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class IonCrystal_Craftable
{

    public static PrefabInfo craftableCrystalInfo;
    
    public static void Register()
    {
        string classID = "38ebd2e5-9dcc-4d7a-ada4-86a22e01191a";
        string filePath = "WorldEntities/Natural/PrecursorIonCrystal.prefab";
        PrefabInfo info = new PrefabInfo(classID, filePath, TechType.PrecursorIonCrystal);

        ICustomPrefab matrix = new CustomPrefab(info);
        var patch = new CustomPrefab("IonCrystal_Placeholder", "", "");
        
        craftableCrystalInfo = patch.Info;
        
        patch.SetGameObject(Plugin.GeneralAssetBundle.LoadAsset<GameObject>("Empty"));
        patch.AddGadget(new ScanningGadget(matrix, Prototype_Craftable.SubInfo.TechType));

        var recipeData = ROTACompatManager.GetRelevantRecipe("PrecursorIonCrystal.json");
        patch.AddGadget(new CraftingGadget(matrix, recipeData)
            .WithCraftingTime(5f)
            .WithFabricatorType(PrecursorFabricator.precursorFabricatorType)
            .WithStepsToFabricatorTab("PowerSources"));

        patch.Register();
    }
}
