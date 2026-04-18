using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using Nautilus.Utility;
using PrototypeSubMod.MiscMonobehaviors;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using UnityEngine;

namespace PrototypeSubMod.Registration;

internal static class BiomeRegisterer
{
    public const string TransmissionSiteBiome = "transmissionsite";
    public const string TransmissionRunupBiome = "transmissionrunup_protovoid";
    
    public static void Register()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        
        var settings = BiomeUtils.CreateBiomeSettings(new Vector3(18, 15, 13), 1.1f, Color.white, 0.15f, Color.white, 0, temperature: 10);

        BiomeHandler.RegisterBiome(Plugin.DEFENSE_CHAMBER_BIOME_NAME, settings, new BiomeHandler.SkyReference("SkyMountains"));
        BiomeHandler.AddBiomeMusic(Plugin.DEFENSE_CHAMBER_BIOME_NAME, AudioUtils.GetFmodAsset("DefenseFacilityExterior"));
        
        #region Tunnel Biomes
        var tunnelSettings = BiomeUtils.CreateBiomeSettings(new Vector3(20, 20, 20), 1f, Color.white, 0.12f, Color.white, 0, startDistance: 20);

        BiomeHandler.RegisterBiome("protodefensetunnel1", tunnelSettings, new BiomeHandler.SkyReference("SkyCrashZone"));
        BiomeHandler.AddBiomeMusic("protodefensetunnel1", AudioUtils.GetFmodAsset("DefenseTunnelMusic1"), FMODGameParams.InteriorState.OnlyOutside);
        BiomeHandler.RegisterBiome("protodefensetunnel2", tunnelSettings, new BiomeHandler.SkyReference("SkyCrashZone"));
        BiomeHandler.AddBiomeMusic("protodefensetunnel2", AudioUtils.GetFmodAsset("DefenseTunnelMusic2"), FMODGameParams.InteriorState.OnlyOutside);
        BiomeHandler.RegisterBiome("protodefensetunnel3", tunnelSettings, new BiomeHandler.SkyReference("SkyCrashZone"));
        BiomeHandler.AddBiomeMusic("protodefensetunnel3", AudioUtils.GetFmodAsset("DefenseTunnelMusic3"), FMODGameParams.InteriorState.OnlyOutside);
        BiomeHandler.RegisterBiome("protodefensetunnel4", tunnelSettings, new BiomeHandler.SkyReference("SkyCrashZone"));
        BiomeHandler.AddBiomeMusic("protodefensetunnel4", AudioUtils.GetFmodAsset("DefenseTunnelMusic4"), FMODGameParams.InteriorState.OnlyOutside);
        BiomeHandler.RegisterBiome("protodefensetunnel5", tunnelSettings, new BiomeHandler.SkyReference("SkyCrashZone"));
        BiomeHandler.AddBiomeMusic("protodefensetunnel5", AudioUtils.GetFmodAsset("DefenseTunnelMusic5"), FMODGameParams.InteriorState.OnlyOutside);
        #endregion

        #region Interceptor Island
        var islandSettings = BiomeUtils.CreateBiomeSettings(new Vector3(40, 15, 9), 0.4f, Color.white, 0.12f, Color.white, 0, 25, 1.4f);
        BiomeHandler.RegisterBiome("interceptorisland", islandSettings, new BiomeHandler.SkyReference("SkyCrashZone"));
        BiomeHandler.AddBiomeMusic("interceptorisland", AudioUtils.GetFmodAsset("ProtoIslandMusic"), FMODGameParams.InteriorState.OnlyOutside);
        #endregion

        #region Engine Facility

        var engineSettings = BiomeUtils.CreateBiomeSettings(new Vector3(35, 7f, 5.5f), 0.4f, Color.white, 0.15f,
            Color.clear,
            1f, 25, 0f, 0f, 24);
        BiomeHandler.RegisterBiome(Plugin.ENGINE_FACILITY_BIOME_NAME, engineSettings, new BiomeHandler.SkyReference("SkyBloodKelpTwo"));
        BiomeHandler.AddBiomeMusic(Plugin.ENGINE_FACILITY_BIOME_NAME, AudioUtils.GetFmodAsset("EngineFacilityMusic"));

        #endregion
        
        #region Warp Core

        var warpCoreSettings = BiomeUtils.CreateBiomeSettings(new Vector3(4.0f, 2.0f, 1.3f), 0f, Color.white, 0.5f,
            new Color(0f, 0.561f, 0.376f),0.05f,
            25f, 0f, 0f, 30);
        BiomeHandler.RegisterBiome("warpcore", warpCoreSettings, new BiomeHandler.SkyReference("SkyLostRiver_Junction"));
        BiomeHandler.AddBiomeMusic("warpcore", AudioUtils.GetFmodAsset("WarpCoreMusic"));

        #endregion

        #region Hull Facility
        var hullSettings =
            BiomeUtils.CreateBiomeSettings(new Vector3(16, 12, 6), 2f, new Color(0, 1, 0.912f), 
                0.25f, new Color(0, 0.95f, 1),
                0.03f, 40, 0f, 0f);
        BiomeHandler.RegisterBiome("protohullfacilitycalm", hullSettings, new BiomeHandler.SkyReference("SkyPrecursorInterior_NoLightmaps"));
        BiomeHandler.AddBiomeMusic("protohullfacilitycalm",
            AudioUtils.GetFmodAsset("HullFacility_Calm"));

        BiomeHandler.RegisterBiome("protohullfacilitytense", hullSettings, new BiomeHandler.SkyReference("SkyPrecursorInterior_NoLightmaps"));
        BiomeHandler.AddBiomeMusic("protohullfacilitytense",
            AudioUtils.GetFmodAsset("HullFacility_Tense"));
        #endregion

        #region Story Ping Void
        
        PrefabInfo voidVolumePrefabInfo = PrefabInfo.WithTechType("StoryPingVoidBiome");
        CustomPrefab voidVolumePrefab = new CustomPrefab(voidVolumePrefabInfo);
        AtmosphereVolumeTemplate voidTemplate = new AtmosphereVolumeTemplate(voidVolumePrefabInfo, AtmosphereVolumeTemplate.VolumeShape.Sphere,
            "void", 11, LargeWorldEntity.CellLevel.Global);
        voidTemplate.ModifyPrefab = prefab =>
        {
            var volum = prefab.GetComponent<AtmosphereVolume>();
            prefab.AddComponent<AtmospherePriorityEnsurer>().priority = volum.priority;
            prefab.AddComponent<DestroyOnStoryEnd>();
        };

        voidVolumePrefab.SetGameObject(voidTemplate);
        voidVolumePrefab.Register();

        var voidSpawnInfo = new SpawnInfo(voidVolumePrefabInfo.ClassID, Plugin.StoryEndPos, Quaternion.identity, Vector3.one * 2400);
        CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(voidSpawnInfo);

        #endregion

        #region Puzzle Facilities

        var puzzleSettings = BiomeUtils.CreateBiomeSettings(new Vector3(0, 0, 0), 0.1f, Color.white, 0.01f,
            Color.clear, sunlightScale:0f, ambientScale:0f);
        BiomeHandler.RegisterBiome("protopuzzlefacility", puzzleSettings, new BiomeHandler.SkyReference("SkyPrecursorInterior_NoLightmaps"));
        BiomeHandler.AddBiomeMusic("protopuzzlefacility", AudioUtils.GetFmodAsset("ProtoPuzzleMusic"));

        #endregion
        
        #region Calibration Site

        var calibrationPrefabInfo = PrefabInfo.WithTechType("CalibrationSiteVoidBiome");
        var calibrationVolumePrefab = new CustomPrefab(calibrationPrefabInfo);
        var calibrationTemplate = new AtmosphereVolumeTemplate(calibrationPrefabInfo, AtmosphereVolumeTemplate.VolumeShape.Sphere,
            "protovoid", 20, LargeWorldEntity.CellLevel.Global);
        calibrationTemplate.ModifyPrefab = prefab =>
        {
            var volum = prefab.GetComponent<AtmosphereVolume>();
            prefab.AddComponent<AtmospherePriorityEnsurer>().priority = volum.priority;
        };

        calibrationVolumePrefab.SetGameObject(calibrationTemplate);
        calibrationVolumePrefab.Register();

        var calibrationCenter = new Vector3(-2970, -390, 853);
        var calibrationSpawnInfo = new SpawnInfo(calibrationPrefabInfo.ClassID, calibrationCenter, 
            Quaternion.identity, Vector3.one * 1200);
        CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(calibrationSpawnInfo);
        var calibrationSpawnInfo2 = new SpawnInfo(calibrationPrefabInfo.ClassID,
            new Vector3(-2183.74f, -376.7f, 861.93f), Quaternion.identity, Vector3.one * 546.6f);
        CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(calibrationSpawnInfo2);
        
        #endregion

        #region Transmission Site Runup

        var transmissionRunupPrefabInfo = PrefabInfo.WithTechType("TransmissionSiteRunup");
        var transmissionRunupVolumePrefab = new CustomPrefab(transmissionRunupPrefabInfo);
        var transmissionRunupTemplate = new AtmosphereVolumeTemplate(transmissionRunupPrefabInfo, AtmosphereVolumeTemplate.VolumeShape.Sphere,
            TransmissionRunupBiome, 20, LargeWorldEntity.CellLevel.Global);
        transmissionRunupTemplate.ModifyPrefab = prefab =>
        {
            var volum = prefab.GetComponent<AtmosphereVolume>();
            prefab.AddComponent<AtmospherePriorityEnsurer>().priority = volum.priority;
        };

        transmissionRunupVolumePrefab.SetGameObject(transmissionRunupTemplate);
        transmissionRunupVolumePrefab.Register();
        
        var transmissionRunupSpawnInfo = new SpawnInfo(transmissionRunupPrefabInfo.ClassID, Plugin.TransmissionSitePos, 
            Quaternion.identity, Vector3.one * 2500);
        CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(transmissionRunupSpawnInfo);
        
        #endregion
        
        #region Transmission Site

        var transmissionPrefabInfo = PrefabInfo.WithTechType("ProtoTransmissionSite");
        var transmissionVolumePrefab = new CustomPrefab(transmissionPrefabInfo);
        var transmissionTemplate = new AtmosphereVolumeTemplate(transmissionPrefabInfo, AtmosphereVolumeTemplate.VolumeShape.Sphere,
            TransmissionSiteBiome, 300, LargeWorldEntity.CellLevel.Global);
        transmissionTemplate.ModifyPrefab = prefab =>
        {
            var volum = prefab.GetComponent<AtmosphereVolume>();
            prefab.AddComponent<AtmospherePriorityEnsurer>().priority = volum.priority;
            var biomeScaler = prefab.AddComponent<IncreaseSizeOnBiomeEnter>();
            biomeScaler.SetInfo(TransmissionSiteBiome, 3f);
        };

        transmissionVolumePrefab.SetGameObject(transmissionTemplate);
        transmissionVolumePrefab.Register();
        
        var transmissionSpawnInfo = new SpawnInfo(transmissionPrefabInfo.ClassID, Plugin.TransmissionSitePos, 
            Quaternion.identity, Vector3.one * 300);
        CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(transmissionSpawnInfo);

        var transmissionSiteSettings = BiomeUtils.CreateBiomeSettings(new Vector3(150, 27.435f, 5.295f), 10f, Color.white, 10f,
            Color.black, startDistance: 15f);
        BiomeHandler.RegisterBiome(TransmissionSiteBiome, transmissionSiteSettings,
            new BiomeHandler.SkyReference("SkyMountains"));
        
        #endregion

        sw.Stop();
        Plugin.Logger.LogInfo($"Biomes registered in {sw.ElapsedMilliseconds}ms");
    }
}
