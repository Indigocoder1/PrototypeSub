using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using PrototypeSubMod.Prefabs.Factors;
using PrototypeSubMod.Utility;
using UnityEngine;
using UWE;
using Object = UnityEngine.Object;

namespace PrototypeSubMod.Prefabs;

internal class PrecursorFabricator
{
    
    public static PrefabInfo prefabInfo { get; private set; }
    public static CraftTree.Type precursorFabricatorType { get; private set; }

    private static CustomPrefab prefab;
    private static List<Tuple<String, GameObject>> customSkyPrefabs = new();

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoPrecursorFabricator");

        prefab = new CustomPrefab(prefabInfo);
        var purpleIcon = SpriteManager.Get(TechType.PrecursorKey_Purple);
        
        prefab.CreateFabricator(out CraftTree.Type fabType)
            .AddTabNode("Keys", Language.main.Get("ProtoPrecursorFabricator_Tab_Keys"), purpleIcon)
            .AddCraftNode(TechType.PrecursorKey_Purple, "Keys")
            .AddCraftNode(TechType.PrecursorKey_Blue, "Keys")
            .AddCraftNode(TechType.PrecursorKey_Orange, "Keys")
            .AddCraftNode(HullFacilityKey.prefabInfo.TechType, "Keys")
            .AddCraftNode(DefenseFacilityKey.prefabInfo.TechType, "Keys")
            .AddCraftNode(EngineFacilityKey.prefabInfo.TechType, "Keys")
            .AddCraftNode(InterceptorFacilityKey.prefabInfo.TechType, "Keys")
            
            .AddTabNode("Resources", Language.main.Get("ProtoPrecursorFabricator_Tab_Resources"), SpriteManager.Get(TechType.PrecursorIonCrystal))
            .AddCraftNode(TechType.PrecursorIonCrystal, "Resources")
            .AddCraftNode(TechType.PrecursorIonCrystalMatrix, "Resources")
            .AddCraftNode(IonPrism_Craftable.prefabInfo.TechType, "Resources")
            .AddCraftNode(AlienBuildingBlock.AlienBuildingBlock.prefabInfo.TechType, "Resources")
            .AddCraftNode(PrecursorIngot_Craftable.prefabInfo.TechType, "Resources")
            
            .AddTabNode("Devices", Language.main.Get("ProtoPrecursorFabricator_Tab_Devices"), SpriteManager.Get(DeployableLight_Craftable.prefabInfo.TechType))
            .AddCraftNode(DeployableLight_Craftable.prefabInfo.TechType, "Devices")
            .AddCraftNode(TechType.PrecursorIonBattery, "Devices")
            .AddCraftNode(TechType.PrecursorIonPowerCell, "Devices")
            .AddCraftNode(BlinkFactor.prefabInfo.TechType, "Devices")
            .AddCraftNode(TetherFactor.prefabInfo.TechType, "Devices")
            .AddCraftNode(SuitColorFactor.prefabInfo.TechType, "Devices")
            .AddCraftNode(PrecursorSuit.prefabInfo.TechType, "Devices")
            .AddCraftNode(PrecursorPropulsionGloves.PrefabInfo.TechType, "Devices")
        
            .AddTabNode("PhaseGate", Language.main.Get("ProtoPrecursorFabricator_Tab_PhaseGate"), SpriteManager.Get(ProtoPhaseGateItem.PrefabInfo.TechType))
            .AddCraftNode(ProtoPhaseGateStabilizer.PrefabInfo.TechType, "PhaseGate")
            .AddCraftNode(ProtoPhaseGateStructure.PrefabInfo.TechType, "PhaseGate")
            .AddCraftNode(ProtoPhaseGateTransmitter.PrefabInfo.TechType, "PhaseGate")
            .AddCraftNode(ProtoPhaseGateItem.PrefabInfo.TechType, "PhaseGate");
        precursorFabricatorType = fabType;
        
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefab)
    {
        var returnPrefab = Plugin.AssetBundle.LoadAsset<GameObject>("PrecursorFabricator.prefab");
        
        if(returnPrefab == null)
            Plugin.Logger.LogError("Failed to load the PrecursorFabricator prefab.");

        returnPrefab.GetComponent<PrefabIdentifier>().ClassId = prefabInfo.TechType.ToString();
        returnPrefab.GetComponent<TechTag>().type = prefabInfo.TechType;
        returnPrefab.SetActive(false);
        
        var instance = Object.Instantiate(returnPrefab);

        //Material setup
        MaterialUtils.ApplySNShaders(instance);
        
        //Fabricator VFX setup
        var fabTask = CraftData.GetPrefabForTechTypeAsync(TechType.Fabricator);
        yield return fabTask;

        var vFab = fabTask.GetResult();

        var vanillaGhostModel = vFab.GetComponentInChildren<CrafterGhostModel>();
        var customGhostModel = instance.GetComponent<CrafterGhostModel>();
        
        customGhostModel._EmissiveTex = vanillaGhostModel._EmissiveTex;
        customGhostModel._NoiseTex = vanillaGhostModel._NoiseTex;

        var vanillaSparks = vFab.GetComponent<Fabricator>().sparksPS;
        var customSparks = UWE.Utils.InstantiateDeactivated(vanillaSparks);

        foreach (var renderer in customSparks.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            renderer.materials[0].SetColor("_Color", new Color(0f, 0.904f, 0.47f, 1f));
        }

        instance.GetComponentInChildren<AlienFabricator>().fxSparksPrefab = customSparks;

        //Forcefield setup
        var forceFieldTask = PrefabDatabase.GetPrefabAsync("2d72ad6c-d30d-41be-baa7-0c1dba757b7c");
        
        yield return forceFieldTask;

        forceFieldTask.TryGetPrefab(out var forceFieldPrefab);

        yield return ProtoMatDatabase.ReplaceVanillaMats(instance);
        
        var lerpColors = instance.GetComponentsInChildren<VFXLerpColor>();
        var vanillaLerpColor = forceFieldPrefab.GetComponentInChildren<VFXLerpColor>();

        foreach (var lerpColor in lerpColors)
        {
            lerpColor.blendCurve = vanillaLerpColor.blendCurve;
            lerpColor.GetComponent<MeshRenderer>().materials[0].SetFloat("_InvFade", 60f);
        }
        
        prefab.Set(instance);
    }

}