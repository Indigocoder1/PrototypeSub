using System.Collections;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.Prefabs.AlienBuildingBlock;
using PrototypeSubMod.Prefabs.FacilityProps;
using PrototypeSubMod.Prefabs.FacilityProps.Hull;
using PrototypeSubMod.Prefabs.Factors;
using PrototypeSubMod.Prefabs.PhaseGates;
using PrototypeSubMod.Prefabs.WormSpawns;
using UnityEngine;

namespace PrototypeSubMod.Registration;

internal static class PrefabRegisterer
{
    public static IEnumerator Register()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        
        ProtoPhaseGate.Register();
        ProtoPhaseGateStabilizer.Register();
        ProtoPhaseGateStructure.Register();
        ProtoPhaseGateTransmitter.Register();
        ProtoPhaseGateItem.Register();
        
        WarperRemnant.Register();
        AlienBuildingBlock.Register();
        BlinkFactor.Register();
        TetherFactor.Register();
        TetherFactorMarker.Register();
        BiomechanicsFactor.Register();
        SuitColorFactor.Register();
        PrecursorIngot_Craftable.Register();
        IonPrism_Craftable.Register();

        yield return new WaitForEndOfFrame();
        
        Prototype_Craftable.Register();
        //ProtoBuildTerminal_World.Register();
        DeployableLight_Craftable.Register();
        ProtoAggressiveWyrm.Register();
        ProtoTransmissionDevice.Register();
        
        yield return new WaitForEndOfFrame();
        
        ListeningDevice_Craftable.Register();
        ProtoRepairBot_Spawned.Register();
        DeactivatedTeleporter_World.Register();
        ProtoEngineFacilityRoom.Register();
        PrecursorCross.Register();
        PrecursorRadio.Register();
        
        yield return new WaitForEndOfFrame();
        
        InterceptorFacilityKey.Register();
        DefenseStoryGoalTrigger_World.Register();
        DecorativeForceFieldArchway.Register();
        NonfunctionalKeyTerminal.Register();
        PrecursorIonCrystal_Craftable.Register();
        OrionFragmentor_World.Register();
        HullFacilityKey.Register();
        DefenseFacilityKey.Register();
        EngineFacilityKey.Register();
        
        yield return new WaitForEndOfFrame();
        
        HullOutpostTeleporter.Register();
        ObstructionRockLoadFar.Register();
        PrototypeSpawner.Register();
        CrystalMatrix_Craftable.Register();
        IonCrystal_Craftable.Register();
        PrecursorPropulsionGloves.Register();
        PrecursorSuit.Register();
        ProtoFacilitySpawner.Register("ProtoHullFacilitySpawner", "HullFacilitySpawner");
        ProtoFacilitySpawner.Register("ProtoEngineFacilitySpawner", "EngineFacilitySpawner");
        ProtoFacilitySpawner.Register("ProtoDefenseFacilitySpawner", "DefenseFacilitySpawner");
        ProtoFacilitySpawner.Register("ProtoBuildTerminalSpawner", "BuildTerminalSpawner");
        
        yield return new WaitForEndOfFrame();
        
        PrecursorFabricator.Register();
        PrecursorPhaseGate.Register();

        StoryGoalTerminal.CreateTerminal("AlienBuildingBlockTerminal", "AlienBuildingBlockEncy");
        StoryGoalTerminal.CreateTerminal("HullFacilityLogsTerminal", "HullFacilityLogsEncy");
        StoryGoalTerminal.CreateTerminal("OrionDataTerminal", "OrionEndeavorsEncy");
        StoryGoalTerminal.CreateTerminal("HullFacilityWormTerminal", "HullFacilityWormTerminalEncy");
        StoryGoalTerminal.CreateTerminal("AnimateEntropyTerminal", "AnimateEntropyTerminalEncy");
        StoryGoalTerminal.CreateTerminal("FragmentationTerminal", "FragmentationTerminalEncy");

        yield return new WaitForEndOfFrame();
        
        NonScanWaterPipe.Register();
        WatergateBlock.Register();
        ProtoPlaque_World.Register();
        ProtoLogo_World.Register();
        DamagedProtoLogo_World.Register();
        TeleporterTerminal_World.Register();
        SmashedDisplayCase_World.Register();
        
        yield return new WaitForEndOfFrame();
        
        NonScannableProp.Register("11e731e7-bc82-4f94-90be-5db7b58b449b", "EmptyDisplayCase");
        NonScannableProp.Register("4f5905f8-ea50-49e8-b24f-44139c6bddcf", "PrecursorScannerArmNoScan1");
        NonScannableProp.Register("ebc943e4-200c-4789-92f3-e675cd982dbe", "PrecursorScannerArmNoScan2");
        NonScannableProp.Register("ac2b0798-e311-4cb1-9074-fae59cd7347a", "PrecursorScannerArmNoScan3");
        NonScannableProp.Register("d3645d71-518d-4546-9b68-a3352b07399a", "EmptyMultiDisplayCase");
        KinematicPrefabClone.Register(IonPrism_Craftable.prefabInfo.ClassID, "KinematicIonPrism");
        KinematicPrefabClone.Register("4af48036-40ba-46b1-a398-ede0bb106213", "KinematicLavaBoomerang");
        KinematicPrefabClone.Register("5f6d9ad1-540d-44b1-b62d-2478cd041ae5", "KinematicLavaEyeEye");
        KinematicPrefabClone.Register("a9da9324-84ed-4a51-9ed3-a0969f455067", "KinematicPeeper");
        KinematicPrefabClone.Register("0db5b44d-19f1-4349-9e1f-04da097010f3", "KinematicBoomerang");
        KinematicPrefabClone.Register("b1d88c87-fd48-495b-a707-e91dc4259858", "KinematicHoverfish");
        KinematicPrefabClone.Register("5de7d617-c04c-4a83-b663-ebf1d3dd90a1", "KinematicGarryfish");
        KinematicPrefabClone.Register("ba851576-86df-48e5-a0be-5cd7ba6f4617", "KinematicSpadefish");
        KinematicPrefabClone.Register("38ebd2e5-9dcc-4d7a-ada4-86a22e01191a", "KinematicIonCrystal");
        KinematicPrefabClone.Register("f90d7d3c-d017-426f-af1a-62ca93fae22e", "KinematicIonCrystalMatrix");

        yield return new WaitForEndOfFrame();
        
        GrayRock01.Register();
        PrecursorCube1Prop.Register();
        PrecursorGunProp.Register();
        ProtoTerminalVFX.Register();
        
        DisplayCaseProp.Register(IonPrism_Craftable.prefabInfo.ClassID, "IonPrism_DisplayCase",
            IonPrism_Craftable.prefabInfo.TechType, new Vector3(0, 1.3f, 0), Vector3.one * 10f);
        DisplayCaseProp.Register(DeployableLight_Craftable.prefabInfo.ClassID, "DeployableLight_DisplayCase",
            DeployableLight_Craftable.prefabInfo.TechType, new Vector3(0, 1.3f, 0), Vector3.one * 0.25f, new[] { "VolumetricLight" });
        DisplayCaseProp.Register("f90d7d3c-d017-426f-af1a-62ca93fae22e", "IonCrystalMatrix_DisplayCase",
            TechType.PrecursorIonCrystalMatrix, new Vector3(0, 1.3f, 0), Vector3.one * 1.3f);
        DisplayCaseProp.Register(AlienBuildingBlock.prefabInfo.ClassID, "AlienBuildingBlock_DisplayCase",
            TechType.PrecursorIonCrystalMatrix, new Vector3(0, 1.3f, 0), Vector3.one);

        ProtoWormSpawnEvent.RegisterEvent("WormSpawnEventVertical",
            Plugin.AssetBundle.LoadAsset<GameObject>("WormSpawnEventVertical"),
            new LootDistributionData.BiomeData[]
            {
                new() { biome = BiomeType.Dunes_SandDune, probability = 0.02f, count = 1 },
                new() { biome = BiomeType.Dunes_SandPlateau, probability = 0.02f, count = 1 },
                new() { biome = BiomeType.BloodKelp_TrenchWall, probability = 0.02f, count = 1},
                new() { biome = BiomeType.BloodKelp_Floor, probability = 0.02f, count = 1},
                new() { biome = BiomeType.GrandReef_Ground, probability = 0.02f, count = 1},
                new() { biome = BiomeType.Mountains_Rock, probability = 0.02f, count = 1},
                new() { biome = BiomeType.SeaTreaderPath_Rock, probability = 0.02f, count = 1},
                new() { biome = BiomeType.SparseReef_Sand, probability = 0.02f, count = 1},
                new() { biome = BiomeType.InactiveLavaZone_Chamber_Floor, probability = 0.02f, count = 1},
            });
        
        Plugin.StoryEndPingTechType = CustomPing.CreatePing("StoryEndPingType", PingType.Signal);
        
        var AssetBundle = Plugin.AssetBundle;
        Texture2D dogIco = AssetBundle.LoadAsset<Texture2D>("dogPosterIcon");
        new CustomPoster("ProtoDogPoster", null, null, AssetBundle.LoadAsset<Texture2D>("DogPoster"), dogIco);
        Texture2D regular1Ico = AssetBundle.LoadAsset<Texture2D>("RegularIcon1");
        new CustomPoster("HamCheesePoster1", null, null, AssetBundle.LoadAsset<Texture2D>("HamAndCheesePoster1_Small"), regular1Ico);
        Texture2D regular2Ico = AssetBundle.LoadAsset<Texture2D>("RegularIcon2");
        new CustomPoster("HamCheesePoster2", null, null, AssetBundle.LoadAsset<Texture2D>("RegularPoster2"), regular2Ico, TechType.PosterExoSuit1);
        
        sw.Stop();
        Plugin.Logger.LogInfo($"Normal prefabs loaded in {sw.ElapsedMilliseconds}ms");
    }
}
