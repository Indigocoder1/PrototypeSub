using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Factors;
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
        var armsTex = Plugin.AssetBundle.LoadAsset<Texture2D>("PrecursorSuitArms");
        var bodyEmission = Plugin.AssetBundle.LoadAsset<Texture2D>("PrecursorSuitEmission");
        var armsEmission = Plugin.AssetBundle.LoadAsset<Texture2D>("PrecursorArmsEmission");
        
        template.ModifyPrefab += gameObject =>
        {
            var renderer = gameObject.GetComponentInChildren<Renderer>();

            renderer.materials[0].SetTexture("_MainTex", bodyTex);
            renderer.materials[0].SetTexture(ShaderPropertyID._SpecTex, bodyTex);
            renderer.materials[0].SetTexture(ShaderPropertyID._Illum, bodyEmission);
            renderer.materials[1].SetTexture("_MainTex", armsTex);
            renderer.materials[1].SetTexture(ShaderPropertyID._SpecTex, armsTex);
            renderer.materials[1].SetTexture(ShaderPropertyID._Illum, armsEmission);

            gameObject.AddComponent<FactorIonManager>();

            GameObject.DestroyImmediate(gameObject.GetComponent<Stillsuit>());
        };
        
        prefab.SetGameObject(template);
        
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe($"{PrefabInfo.ClassID}.json"));
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);
        prefab.SetEquipment(EquipmentType.Body);

        prefab.Register();

        var suitTextures = new Dictionary<string, Texture2D>
        {
            { "_MainTex", bodyTex },
            { "_SpecTex", bodyTex },
            { "_Illum", bodyEmission }
        };
        
        var armTextures = new Dictionary<string, Texture2D>
        {
            { "_MainTex", armsTex },
            { "_SpecTex", armsTex },
            { "_Illum", armsEmission }
        };

        var suit = new ModdedSuit(suitTextures, armTextures, ModdedSuitsManager.VanillaModel.WaterFiltration,
            PrefabInfo.TechType, ModdedSuitsManager.Modifications.Reinforced, tempValue: 25f);
        ModdedSuitsManager.AddModdedSuit(suit);
    }
}