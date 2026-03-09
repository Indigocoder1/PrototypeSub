using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using PrototypeSubMod.Compatibility;
using SuitLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameObjectPoolPrefabMap;

namespace PrototypeSubMod.Prefabs;

public static class PrecursorPropulsionGloves
{
    public static PrefabInfo PrefabInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("PrecursorPropulsionGloves", null, null, "English")
            .WithSizeInInventory(new Vector2int(2, 2))
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("PrecursorPropulsionGlovesIcon.png"));

        var prefab = new CustomPrefab(PrefabInfo);

        var template = new CloneTemplate(PrefabInfo, TechType.ReinforcedGloves);
        var glovesTex = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("PrecursorSuitGloves");
        template.ModifyPrefab += prefab =>
        {
            var material = prefab.GetComponentInChildren<Renderer>().material;
            material.SetTexture("_MainTex", glovesTex);
            material.SetTexture("_SpecTex", glovesTex);
        };
        prefab.SetGameObject(template);
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe($"{PrefabInfo.ClassID}.json"));
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);
        prefab.SetEquipment(EquipmentType.Gloves);

        CraftDataHandler.SetBackgroundType(PrefabInfo.TechType, CraftData.BackgroundType.PlantAirSeed);

        prefab.Register();
        
        var armTextures = new Dictionary<string, Texture2D>
        {
            { "_MainTex", glovesTex },
            { "_SpecTex", glovesTex }
        };

        var gloves = new ModdedGloves(armTextures, ModdedSuitsManager.VanillaModel.Reinforced,
            PrefabInfo.TechType, ModdedSuitsManager.Modifications.Reinforced, tempValue: 12);
        ModdedSuitsManager.AddModdedGloves(gloves);
    }
}