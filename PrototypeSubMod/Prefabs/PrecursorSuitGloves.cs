using System.Collections.Generic;
using System.Linq;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using SuitLib;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public static class PrecursorSuitGloves
{
    public static PrefabInfo PrefabInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("PrecursorSuitGloves", null, null, "English");

        var prefab = new CustomPrefab(PrefabInfo);

        var template = new CloneTemplate(PrefabInfo, TechType.ReinforcedGloves);
        var glovesTex = Plugin.AssetBundle.LoadAsset<Texture2D>("PrecursorSuitGloves");
        template.ModifyPrefab += prefab =>
        {
            var material = prefab.GetComponentInChildren<Renderer>().material;
            material.SetTexture("_MainTex", glovesTex);
            material.SetTexture("_SpecTex", glovesTex);
        };
        prefab.SetGameObject(template);
        prefab.SetEquipment(EquipmentType.Gloves);

        prefab.Register();
        
        var armTextures = new Dictionary<string, Texture2D>
        {
            { "_MainTex", glovesTex }
        };

        var gloves = new ModdedGloves(armTextures, ModdedSuitsManager.VanillaModel.Reinforced,
            PrefabInfo.TechType, ModdedSuitsManager.Modifications.Reinforced);
        ModdedSuitsManager.AddModdedGloves(gloves);
    }
}