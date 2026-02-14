using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using PrototypeSubMod.Compatibility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class CrystalMatrix_Craftable
{

    public static PrefabInfo craftableCrystalMatrixInfo;
    
    public static void Register()
    {
        string classID = "f90d7d3c-d017-426f-af1a-62ca93fae22e";
        string filePath = "WorldEntities/EnvironmentResources/PrecursorIonCrystalMatrix.prefab";
        PrefabInfo info = new PrefabInfo(classID, filePath, TechType.PrecursorIonCrystalMatrix);

        ICustomPrefab matrix = new CustomPrefab(info);
        var patch = new CustomPrefab("Proto_MatrixPlaceholder", "", "");
        
        craftableCrystalMatrixInfo = patch.Info;

        var template = new CloneTemplate(patch.Info, TechType.PrecursorIonCrystalMatrix)
        {
            ModifyPrefab = prefab =>
            {
                List<GameObject> matrixCubes = new();

                for (int i = 1; i < 6; i++)
                {
                    matrixCubes.Add(prefab.transform.GetChild(i).gameObject);
                }

                var modelObject = Object.Instantiate(new GameObject("model"), prefab.transform);

                foreach (var cube in matrixCubes)
                {
                    cube.transform.SetParent(modelObject.transform);
                }
                
                var vfxFabricating = modelObject.AddComponent<VFXFabricating>();

                vfxFabricating.localMinY = -0.4f;
                vfxFabricating.localMaxY = 0.4f;
                vfxFabricating.posOffset = new Vector3(0f, -0.05f, 0.05f);
                vfxFabricating.eulerOffset = new Vector3(300f, 180f, 180f);
                vfxFabricating.scaleFactor = 0.6f;
            }
        };
        
        patch.SetGameObject(template);
        patch.AddGadget(new ScanningGadget(matrix, Prototype_Craftable.SubInfo.TechType));

        var recipeData = ROTACompatManager.GetRelevantRecipe("PrecursorIonCrystalMatrix.json");
        patch.AddGadget(new CraftingGadget(matrix, recipeData)
            .WithCraftingTime(5f)
            .WithFabricatorType(PrecursorFabricator.precursorFabricatorType)
            .WithStepsToFabricatorTab("PowerSources"));
    patch.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);

        Sprite matrixSprite = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("matrixSprite");
        SpriteHandler.RegisterSprite(TechType.PrecursorIonCrystalMatrix, matrixSprite);



        CraftDataHandler.SetPickupSound(TechType.PrecursorIonCrystalMatrix, "event:/loot/pickup_precursorioncrystal");

        patch.Register();
    }
}
