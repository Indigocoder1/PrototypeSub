using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Factors;
using PrototypeSubMod.PrecursorWearables;
using SuitLib;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public static class PrecursorSuit
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("PrecursorSuit", null, null, "English")
            .WithSizeInInventory(new Vector2int(2, 2))
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("PrecursorSuitIcon.png"));

        var prefab = new CustomPrefab(prefabInfo);

        var template = new CloneTemplate(prefabInfo, TechType.WaterFiltrationSuit);
        var bodyTex = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("PrecursorSuitBody");
        var armsTex = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("PrecursorSuitArms");
        var bodyEmission = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("PrecursorSuitEmission");
        var armsEmission = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("PrecursorArmsEmission");
        
        template.ModifyPrefab += gameObject =>
        {
            var renderer = gameObject.GetComponentInChildren<Renderer>();

            renderer.materials[0].SetTexture(ShaderPropertyID._MainTex, bodyTex);
            renderer.materials[0].SetTexture(ShaderPropertyID._SpecTex, bodyTex);
            renderer.materials[0].SetTexture(ShaderPropertyID._Illum, bodyEmission);
            renderer.materials[1].SetTexture(ShaderPropertyID._MainTex, armsTex);
            renderer.materials[1].SetTexture(ShaderPropertyID._SpecTex, armsTex);
            renderer.materials[1].SetTexture(ShaderPropertyID._Illum, armsEmission);

            gameObject.AddComponent<FactorIonManager>();
            gameObject.AddComponent<PrecursorSuitEnergyDisplay>();

            GameObject.DestroyImmediate(gameObject.GetComponent<Stillsuit>());
        };
        
        prefab.SetGameObject(template);
        
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe($"{prefabInfo.ClassID}.json"));
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);
        prefab.SetEquipment(EquipmentType.Body);

        CraftDataHandler.SetBackgroundType(prefabInfo.TechType, CraftData.BackgroundType.PlantAirSeed);

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

        if (Chainloader.PluginInfos.ContainsKey("com.github.tinyhoot.DeathrunRemade"))
        {
            var deathrunAPI = HarmonyLib.AccessTools.TypeByName("DeathrunRemade.DeathrunAPI");

            var addSuitCrushDepthMethod = deathrunAPI.GetMethod("AddSuitCrushDepth", new [] { typeof(TechType), typeof(IEnumerable<float>) });
            addSuitCrushDepthMethod.Invoke(null, new object[] { prefabInfo.TechType, new [] { 2400f, 2000f } });
        }

        var suit = new ModdedSuit(suitTextures, armTextures, ModdedSuitsManager.VanillaModel.WaterFiltration,
            prefabInfo.TechType, ModdedSuitsManager.Modifications.Reinforced, tempValue: 25f);
        ModdedSuitsManager.AddModdedSuit(suit);
    }
}