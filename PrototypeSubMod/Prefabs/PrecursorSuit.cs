using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using PrototypeSubMod.Compatibility;
using SuitLib;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public static class PrecursorSuit
{
    public static PrefabInfo PrefabInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("PrecursorSuit", null, null, "English");

        var prefab = new CustomPrefab(PrefabInfo);

        var template = new CloneTemplate(PrefabInfo, TechType.WaterFiltrationSuit);
        var bodyTex = Plugin.AssetBundle.LoadAsset<Texture2D>("PrecursorSuitBody");
        var bodySpec = Plugin.AssetBundle.LoadAsset<Texture2D>("PrecursorSuitSpec");
        var armsTex = Plugin.AssetBundle.LoadAsset<Texture2D>("PrecursorSuitArms");
        
        template.ModifyPrefab += gameObject =>
        {
            var renderer = gameObject.GetComponentInChildren<Renderer>();

            renderer.materials[0].SetTexture("_MainTex", bodyTex);
            renderer.materials[0].SetTexture(ShaderPropertyID._SpecTex, bodySpec);
            renderer.materials[1].SetTexture("_MainTex", armsTex);
            renderer.materials[1].SetTexture(ShaderPropertyID._SpecTex, armsTex);
        };
        
        prefab.SetGameObject(template);
        
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe($"{PrefabInfo.ClassID}.json"));
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);
        prefab.SetEquipment(EquipmentType.Body);

        prefab.Register();

        var suitTextures = new Dictionary<string, Texture2D>
        {
            { "_MainTex", bodyTex },
            { "_SpecTex", bodySpec }
        };
        
        var armTextures = new Dictionary<string, Texture2D>
        {
            { "_MainTex", armsTex }
        };

        var suit = new ModdedSuit(suitTextures, armTextures, ModdedSuitsManager.VanillaModel.WaterFiltration,
            PrefabInfo.TechType);
        ModdedSuitsManager.AddModdedSuit(suit);
    }
}