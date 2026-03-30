using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using PrototypeSubMod.Commands;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Patches;
using PrototypeSubMod.PowerSystem;
using PrototypeSubMod.Registration;
using PrototypeSubMod.SaveData;
using PrototypeSubMod.Utility;
using SubLibrary.Audio;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Nautilus.Handlers.LoadingScreen;
using Nautilus.Handlers.TitleScreen;
using Nautilus.Utility;
using Nautilus.Utility.ModMessages;
using PrototypeSubMod.Factors;
using PrototypeSubMod.MiscMonobehaviors;
using PrototypeSubMod.Pathfinding.SaveSystem;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.Prefabs.AlienBuildingBlock;
using PrototypeSubMod.VehicleAccess;
using SubLibrary.Handlers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UWE;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Extensions;
using Nautilus.FMod;

namespace PrototypeSubMod
{
    [BepInPlugin(GUID, pluginName, versionString)]
    [BepInDependency("com.snmodding.nautilus", "1.0.0.49")]
    [BepInDependency("com.indigocoder.sublibrary", "1.7.5")]
    [BepInDependency("Esper89.TerrainPatcher", "1.2.2")]
    [BepInDependency("Indigocoder.SuitLib", "1.1.8")]
    [BepInDependency("ArchitectsLibrary", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.lee23.theredplague", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.danithedani.deepercreatures", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.lee23.epicweather", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.aci.thesilence", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.mikjaw.subnautica.vehicleframework.mod", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.digaoness.CyclopsModules", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string GUID = "com.prototech.prototypesub";
        private const string pluginName = "Prototype Sub";
        private const string versionString = "1.0.7";

        public new static ManualLogSource Logger { get; private set; }

        internal static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

        public static string AssetsFolderPath { get; } = Path.Combine(Path.GetDirectoryName(Assembly.Location), "Assets");
        public static string RecipesFolderPath { get; } = Path.Combine(Path.GetDirectoryName(Assembly.Location), "Recipes");

        public static AssetBundle GeneralAssetBundle { get; private set; }
        public static AssetBundle EasyPrefabBundle { get; private set; }
        public static AssetBundle AudioBundle { get; private set; }
        public static AssetBundle ScenesAssetBundle { get; private set; }
        public static AssetBundle TitleAssetBundle { get; } = AssetBundle.LoadFromFile(Path.Combine(AssetsFolderPath, "prototypetitle"));

        public static AssetBundle ShadersAssetBundle
        {
            get
            {
                if (_shadersAssetBundle != null) return _shadersAssetBundle;

                string bundleName = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? "prototypeshaders_MAC"
                    : "prototypeshaders_WINDOWS";
                _shadersAssetBundle = AssetBundle.LoadFromFile(Path.Combine(AssetsFolderPath, bundleName));

                #if !DEBUG
                string deleteBundleName = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? "prototypeshaders_WINDOWS"
                    : "prototypeshaders_MAC";

                string deletePath = Path.Combine(AssetsFolderPath, deleteBundleName);
                if (File.Exists(deletePath))
                {
                    File.Delete(deletePath);
                }
                #endif
                
                return _shadersAssetBundle;
            }
        }

        private static AssetBundle _shadersAssetBundle;

        public static EquipmentType PrototypePowerType { get; } = EnumHandler.AddEntry<EquipmentType>("PrototypePowerType");
        public static EquipmentType LightBeaconEquipmentType { get; } = EnumHandler.AddEntry<EquipmentType>("LightBeaconType");
        public static EquipmentType PhaseGateEquipmentType { get; } = EnumHandler.AddEntry<EquipmentType>("PhaseGateType");
        public static EquipmentType DummyPowerType { get; } = EnumHandler.AddEntry<EquipmentType>("ProtoDummyPowerType");
        public static EquipmentType FactorEquipmentType { get; } = EnumHandler.AddEntry<EquipmentType>("FactorEquipmentType");

        public static TechGroup PrototypeGroup { get; } = EnumHandler.AddEntry<TechGroup>("PrototypeSub").WithPdaInfo(null);
        public static TechCategory PrototypeCategory { get; } = EnumHandler.AddEntry<TechCategory>("PrototypeSub").RegisterToTechGroup(PrototypeGroup)
            .WithPdaInfo(null);

        public static TechCategory ProtoModuleCategory { get; } = EnumHandler.AddEntry<TechCategory>("ProtoModules").RegisterToTechGroup(PrototypeGroup)
            .WithPdaInfo(null);
        
        public static TechGroup ProtoFabricatorGroup { get; } = EnumHandler.AddEntry<TechGroup>("ProtoFabricator").WithPdaInfo(null);
        public static TechCategory ProtoFabricatorCatgeory { get; } = EnumHandler.AddEntry<TechCategory>("ProtoFabricator").RegisterToTechGroup(ProtoFabricatorGroup)
            .WithPdaInfo(null);
        
        public static PDATab TransmissionEntryTab { get; } = EnumHandler.AddEntry<PDATab>("ProtoTransmissionEntry");
        
        internal static ProtoGlobalSaveData GlobalSaveData = SaveDataHandler.RegisterSaveDataCache<ProtoGlobalSaveData>();
        internal static GameObject welderPrefab;
        
        internal static PingType PrototypePingType { get; private set; }

        internal const string DEFENSE_CHAMBER_BIOME_NAME = "protodefensefacility";
        internal const string ENGINE_FACILITY_BIOME_NAME = "protoenginefacility";
        internal static readonly Vector3 StoryEndPos = new (858, -800, 3116);
        internal static readonly Vector3 TransmissionSitePos = new (-1000, -850, -3250);
        internal static readonly Vector3 TransmissionSiteStartPos = new (-1000, -350, -1600);
        internal static readonly Vector3 DefensePingPos = new (700, -489, -1456);
        internal static readonly Dictionary<string, Vector3> FACILITY_POSITIONS = new()
        {
            { "InterceptorFacility", new Vector3(547, -709, 955) },
            { "DefenseFacility", new Vector3(689, -483, -1404f) },
            { "DefenseMoonpool", new Vector3(782, -460, -1046) },
            { "EngineFacility", new Vector3(-558, -463, 1497f) },
            { "HullFacility", new Vector3(-1182, -443, -1146) },
            { "HullOutpost", new Vector3(-162, -69, -226) },
            { "NumberPuzzle",  new Vector3(-242, -72, 296) },
            { "BearingPuzzle",  new Vector3(1222, -305, 529) },
            { "PPT", new Vector3(449, -92, 1169) },
            { "Worm1", new Vector3(-898, -386, -1284) },
            { "Worm2", new Vector3(-1006, -293, -1148) },
            { "Worm3", new Vector3(-1205, -312, -707) },
            { "Worm4", new Vector3(-1224, -217, -697) },
            { "TransmissionSite", (TransmissionSitePos) }
        };
        internal static TechType StoryEndPingTechType;
        internal static GridSaveData pathfindingGridSaveData;
        internal static event Action<GridSaveData> onLoadGridSaveData;

        private static bool Initialized;
        private static bool PrefabsInitialized;
        private static bool StructuresRegistered;
        private static bool MiscellaneousRegistered;
        private static Harmony harmony = new Harmony(GUID);

        private void Awake()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            // Set project-scoped logger instance
            Logger = base.Logger;
            
            // Register harmony patches, if there are any
            harmony.PatchAll(Assembly);

            StartCoroutine(LoadAudioAsync());
            StartCoroutine(LoadScenesBundle());

            var databaseSW = new System.Diagnostics.Stopwatch();
            databaseSW.Start();
            ProtoMatDatabase.Initalize();
            databaseSW.Stop();
            Logger.LogInfo($"Material database registered in {databaseSW.ElapsedMilliseconds}ms");
            
            LanguageHandler.RegisterLocalizationFolder();
            InputRegisterer.Register();
            
            CompatPatchRegisterer.RegisterCompatibilityPatches(harmony);
            InitializeSlotMapping();
            RegisterTitleAddons();
            
            var miscSW = new System.Diagnostics.Stopwatch();
            miscSW.Start();
            ConsoleCommandsHandler.RegisterConsoleCommands(typeof(PrototypeCommands));
            WeatherCompatManager.Initialize();
            SetupSaveStateReferences.SetupReferences(Assembly);
            miscSW.Stop();
            Logger.LogInfo($"Miscellaneous items registered in {miscSW.ElapsedMilliseconds}ms");
            
            StartCoroutine(Initialize());
            StartCoroutine(MakeSeaTreaderBlockersPassthrough());
            StartCoroutine(LazyInitialize());

            var recipeData = CraftDataHandler.GetRecipeData(TechType.RocketStage3);
            for (int i = 0; i < recipeData.ingredientCount; i++)
            {
                var ingredient = recipeData.Ingredients[i];
                if (ingredient.techType == TechType.CyclopsShieldModule)
                {
                    ingredient = new Ingredient(TechType.ReactorRod, 2);
                }

                recipeData.Ingredients[i] = ingredient;
            }
            
            CraftDataHandler.SetRecipeData(TechType.RocketStage3, recipeData);

            string modName = Language.main.Get("ProtoModName");
            WaitScreenHandler.RegisterEarlyAsyncLoadTask(modName, LoadBundleTask, Language.main.Get("ProtoWaitLoadingBundle"));
            WaitScreenHandler.RegisterEarlyAsyncLoadTask(modName, LoadGeneralPrefabsTask, Language.main.Get("ProtoWaitLoadingPrefabs"));
            WaitScreenHandler.RegisterEarlyAsyncLoadTask(modName, LoadEasyPrefabsTask, Language.main.Get("ProtoWaitLoadingEasyPrefabs"));
            WaitScreenHandler.RegisterEarlyAsyncLoadTask(modName, LoadStructuresTask, Language.main.Get("ProtoWaitRegisteringStructures"));
            WaitScreenHandler.RegisterEarlyAsyncLoadTask(modName, LoadMiscellaneousTask, Language.main.Get("ProtoWaitRegisteringMiscellaneous"));
            WaitScreenHandler.RegisterEarlyAsyncLoadTask(modName, LoadScenesBundle, Language.main.Get("ProtoWaitRegisteringScenes"));
            WaitScreenHandler.RegisterEarlyAsyncLoadTask(modName, LoadAudioBundle, Language.main.Get("ProtoWaitRegisteringAudio"));

            ModMessageSystem.SendGlobal("FindMyUpdates",
                "https://raw.githubusercontent.com/Indigocoder1/PrototypeSub/refs/heads/main/PrototypeSubMod/Version.json");

            sw.Stop();
            Logger.LogInfo($"Plugin {GUID} is loaded in {sw.ElapsedMilliseconds} ms!");
        }

        private IEnumerator Initialize()
        {
            if (Initialized) yield break;
            
            Initialized = true;
            
            yield return new WaitUntil(() => CraftData.cacheInitialized && CraftTree.initialized);
            yield return new WaitForEndOfFrame();

            var task = CraftData.GetPrefabForTechTypeAsync(TechType.Welder);
            yield return task;

            welderPrefab = task.GetResult();

            var ghostTask = PrefabDatabase.GetPrefabAsync("54701bfc-bb1a-4a84-8f79-ba4f76691bef");
            yield return ghostTask;

            if (!ghostTask.TryGetPrefab(out var ghostPrefab)) throw new Exception("Error loading ghost leviathan prefab");

            ghostPrefab.EnsureComponent<GhostLeviathanFacilityManager>();

            var lifepod3PDATask = PrefabDatabase.GetPrefabAsync("c6f6fe72-e16e-4b00-8df2-6b4e1a3533f4");
            yield return lifepod3PDATask;

            if (!lifepod3PDATask.TryGetPrefab(out var lifepod3PDAPrefab)) throw new Exception("Error loading lifepod 3 PDA prefab");

            var storyHandTarget = lifepod3PDAPrefab.GetComponent<StoryHandTarget>();

            storyHandTarget.goal.key = "ProtoLifepod3PDA";

            if (Chainloader.PluginInfos.ContainsKey("com.aotu.returnoftheancients"))
            {
                var guardianTask = PrefabDatabase.GetPrefabAsync("GuardianConstruction_QEP");
                yield return guardianTask;
                
                if (!guardianTask.TryGetPrefab(out var guardianPrefab)) throw new Exception("Error loading RotA guardian prefab");

                guardianPrefab.EnsureComponent<DestroyOnStart>();
            }
        }

        private void Start()
        {
            UWE.CoroutineHost.StartCoroutine(CyclopsReferenceHandler.EnsureCyclopsReference());
        }

        private IEnumerator LoadBundleTask(WaitScreenHandler.WaitScreenTask waitTask)
        {
            waitTask.Status = Language.main.GetFormat("ProtoWaitLoadingBundle");
            yield return new WaitUntil(() => GeneralAssetBundle != null);
        }

        private IEnumerator LoadGeneralPrefabsTask(WaitScreenHandler.WaitScreenTask waitTask)
        {
            waitTask.Status = Language.main.Get("ProtoWaitRegisteringGeneralPrefabs");
            yield return new WaitUntil(() => PrefabRegisterer.PrefabsLoaded);
        }

        private IEnumerator LoadEasyPrefabsTask(WaitScreenHandler.WaitScreenTask waitTask)
        {
            waitTask.Status = Language.main.GetFormat("ProtoWaitRegisteringEasyPrefabs",
                (LoadEasyPrefabs.GetLoadProgress() * 100).ToString("F0"));
            LoadEasyPrefabs.ClearProgressEvents();
            LoadEasyPrefabs.OnProgressChanged += progress =>
            {
                waitTask.Status = Language.main.GetFormat("ProtoWaitRegisteringEasyPrefabs", (progress * 100).ToString("F0"));
            };
            while (!PrefabsInitialized)
            {
                yield return null;
            }
        }

        private IEnumerator LoadStructuresTask(WaitScreenHandler.WaitScreenTask waitTask)
        {
            waitTask.Status = Language.main.Get("ProtoWaitRegisteringStructures");
            yield return new WaitUntil(() => StructuresRegistered);
        }

        private IEnumerator LoadMiscellaneousTask(WaitScreenHandler.WaitScreenTask waitTask)
        {
            waitTask.Status = Language.main.Get("ProtoWaitRegisteringMiscellaneous");
            yield return new WaitUntil(() => MiscellaneousRegistered);
        }

        private IEnumerator LoadAudioBundle(WaitScreenHandler.WaitScreenTask waitTask)
        {
            waitTask.Status = Language.main.Get("ProtoWaitRegisteringAudio");
            yield return new WaitUntil(() => AudioBundle != null);
            PDAMessageRegisterer.Register();
        }

        private IEnumerator LoadScenesBundle(WaitScreenHandler.WaitScreenTask waitTask)
        {
            waitTask.Status = Language.main.Get("ProtoWaitRegisteringScenes");
            yield return new WaitUntil(() => ScenesAssetBundle != null);
        }
        
        private IEnumerator LazyInitialize()
        {
            if (GeneralAssetBundle != null) yield break;

            Logger.LogDebug("Started loading general asset bundle");
            
            var task = AssetBundle.LoadFromFileAsync(Path.Combine(AssetsFolderPath, "prototypeassets"));
            yield return task;
            GeneralAssetBundle = task.assetBundle;
            
            Logger.LogDebug("General asset bundle loaded");
            
            LoadPathfindingGrid();
            
            PrototypePingType = EnumHandler.AddEntry<PingType>("PrototypeSub")
                .WithIcon(GeneralAssetBundle.LoadAsset<Sprite>("Proto_HUD_Marker"));
            
            Logger.LogDebug("Set ping type");
            yield return PrefabRegisterer.Register();
            Logger.LogDebug($"Loaded normal prefabs");
            
            Logger.LogDebug("Loading easy prefab bundle");
            var easyPrefabBundleTask =
                AssetBundle.LoadFromFileAsync(Path.Combine(AssetsFolderPath, "prototypeeasyprefabs"));
            yield return easyPrefabBundleTask;
            EasyPrefabBundle = easyPrefabBundleTask.assetBundle;
            Logger.LogDebug("Easy prefab bundle loaded");
            
            yield return LoadEasyPrefabs.LoadPrefabs(EasyPrefabBundle, EncyEntryRegisterer.Register, GC.Collect, GC.WaitForPendingFinalizers);
            Logger.LogDebug($"Loaded easy prefabs");
            
            PrototypePowerSystem.AllowedPowerSources = new()
            {
                { WarperRemnant.prefabInfo.TechType, new PowerConfigData(1) },
                { AlienBuildingBlock.prefabInfo.TechType, new PowerConfigData(4) },
                { TechType.PrecursorIonCrystal, new PowerConfigData(5) },
                { EngineFacilityKey.prefabInfo.TechType, new PowerConfigData(6) },
                { TechType.PrecursorIonCrystalMatrix, new PowerConfigData(8) },
                { IonPrism_Craftable.prefabInfo.TechType, new PowerConfigData(10) }
            };
            
            //ROTACompatManager.AddCompatiblePowerSources();
            Logger.LogDebug($"Setup power sources");
            
            yield return EnsureBatteryComponents();
            SceneManager.sceneLoaded += OnSceneLoaded;
            Logger.LogDebug($"Setup power source prefabs");
            PrefabsInitialized = true;
            
            yield return StructureRegisterer.Register();
            StructuresRegistered = true;
            
            Logger.LogDebug($"Structures registered");
            
            StoryGoalsRegisterer.Register();
            PuzzleHintRegistration.Register();
            RadioMessageRegisterer.Register();
            BiomeRegisterer.Register();
            LootRegister.Register();
            CommandRegisterer.Register();
            ConsoleCommandsHandler.RegisterConsoleCommands(typeof(CommandRegisterer));
            MiscellaneousRegistered = true;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "MenuEnvironment") return;

            StartCoroutine(EnsureBatteryComponents());
            StartCoroutine(RemoveGuardianPrefab());
        }

        private IEnumerator RemoveGuardianPrefab()
        {
            if (Chainloader.PluginInfos.ContainsKey("com.aotu.returnoftheancients"))
            {
                var guardianTask = PrefabDatabase.GetPrefabAsync("GuardianConstruction_QEP");
                yield return guardianTask;
                
                if (!guardianTask.TryGetPrefab(out var guardianPrefab)) throw new Exception("Error loading RotA guardian prefab");

                guardianPrefab.EnsureComponent<DestroyOnStart>();
            }
        }
        
        private IEnumerator EnsureBatteryComponents()
        {
            foreach (var kvp in PrototypePowerSystem.AllowedPowerSources)
            {
                CoroutineTask<GameObject> prefabTask = CraftData.GetPrefabForTechTypeAsync(kvp.Key);
                yield return prefabTask;

                GameObject prefab = prefabTask.result.Get();
                prefab.EnsureComponent<PrototypePowerBattery>();
            }
        }

        private IEnumerator LoadScenesBundle()
        {
            var task = AssetBundle.LoadFromFileAsync(Path.Combine(AssetsFolderPath, "prototypescenes"));
            yield return task;
            ScenesAssetBundle = task.assetBundle;
        }

        private void RegisterTitleAddons()
        {
            var titleMusic = TitleAssetBundle.LoadAsset<CustomFMODAsset>("ProtoTitleMusic");
            SubAudioLoader.RegisterAssetAudio(titleMusic);
            
            #region Title Screen
            GameObject SpawnObject()
            {
                var holder = new GameObject("ProtoTitleAssets");
                
                var phaseGatesObject = Instantiate(TitleAssetBundle.LoadAsset<GameObject>("TitlePhaseGate"), holder.transform);
                phaseGatesObject.transform.position = new Vector3(0, 6, 50);
                phaseGatesObject.transform.localScale = Vector3.one * 0.6f;
                MaterialUtils.ApplySNShaders(phaseGatesObject);
                StartCoroutine(ProtoMatDatabase.ReplaceVanillaMats(phaseGatesObject));
                
                return holder;
            }
            
            var objectAddon = new WorldObjectTitleAddon(SpawnObject);
            var musicAddon = new MusicTitleAddon(titleMusic);
            var customData = new TitleScreenHandler.CustomTitleData("ProtoModName", objectAddon, musicAddon);

            const string addonName = "ProtoTitleData";
            TitleScreenHandler.RegisterTitleScreenObject(addonName, customData);
            #endregion

            #region Loading Screens

            var constructedData = new LoadingScreenHandler.LoadingScreenData(
                TitleAssetBundle.LoadAsset<Sprite>("ProtoCraftedScreen"), storyGoalRequirement: "PrototypeCrafted");
            var engineData = new LoadingScreenHandler.LoadingScreenData(
                TitleAssetBundle.LoadAsset<Sprite>("EngineFacilityScreen"), 2, storyGoalRequirement: "OnUnlocked_EngineUpgradeText_Native");
            var defenseData = new LoadingScreenHandler.LoadingScreenData(
                TitleAssetBundle.LoadAsset<Sprite>("DefenseFacilityScreen"), 3, storyGoalRequirement: "OnUnlocked_DefenseUpgradeText_Native");
            var interceptorData = new LoadingScreenHandler.LoadingScreenData(
                TitleAssetBundle.LoadAsset<Sprite>("ArchwayFacilityScreen"), 4, storyGoalRequirement: "OnUnlocked_ArchwayUpgradeText_Native");
            var hullData = new LoadingScreenHandler.LoadingScreenData(
                TitleAssetBundle.LoadAsset<Sprite>("HullFacilityScreen"), 5, storyGoalRequirement: "OnUnlocked_HullUpgradeText_Native");
            
            LoadingScreenHandler.RegisterLoadingScreen(addonName, new[]
            {
                constructedData,
                engineData,
                defenseData,
                interceptorData,
                hullData
            });

            #endregion
        }

        private void InitializeSlotMapping()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            
            foreach (string name in PrototypePowerSystem.SLOT_NAMES)
            {
                Equipment.slotMapping.Add(name, PrototypePowerType);
            }

            foreach (var slot in FactorEquipmentManager.FactorSlots)
            {
                Equipment.slotMapping.Add(slot, FactorEquipmentType);
            }

            Equipment.slotMapping.Add(ProtoVehicleAccessTerminal.SLOT_NAME, EquipmentType.NuclearReactor);
            
            sw.Stop();
            Logger.LogInfo($"Slot mapping registered in {sw.ElapsedMilliseconds}ms");
        }

        private IEnumerator LoadAudioAsync()
        {
            var bundleRequest = AssetBundle.LoadFromFileAsync(Path.Combine(AssetsFolderPath, "prototypeaudio"));
            yield return bundleRequest;

            AudioBundle = bundleRequest.assetBundle;
            
            var audioSW = new System.Diagnostics.Stopwatch();
            audioSW.Start();
            var customFmodRequest = AudioBundle.LoadAllAssetsAsync(typeof(CustomFMODAsset));
            yield return customFmodRequest;
            
            foreach (var asset in customFmodRequest.allAssets)
            {
                SubAudioLoader.RegisterAssetAudio((CustomFMODAsset)asset);
            }
            
            var multiFmodRequest = AudioBundle.LoadAllAssetsAsync(typeof(MultiClipFMODAsset));
            yield return multiFmodRequest;

            foreach (var asset in multiFmodRequest.allAssets)
            {
                var multiFMODAsset = (MultiClipFMODAsset)asset;
                var sounds = AudioUtils.CreateSounds(multiFMODAsset.audioClips, multiFMODAsset.mode).ToArray();
                if (multiFMODAsset.minDistance3D > 0 || multiFMODAsset.maxDistance3D > 0)
                {
                    foreach (var sound in sounds)
                    {
                        sound.set3DMinMaxDistance(multiFMODAsset.minDistance3D, multiFMODAsset.maxDistance3D);
                    }
                }

                if (multiFMODAsset.fadeOutTime > 0)
                {
                    foreach (var sound in sounds)
                    {
                        sound.AddFadeOut(multiFMODAsset.fadeOutTime);
                    }
                }

                var multiSoundsEvent = new FModMultiSounds(sounds, multiFMODAsset.GetBus(), multiFMODAsset.randomizePlayOrder);
                CustomSoundHandler.RegisterCustomSound(multiFMODAsset.path, multiSoundsEvent);
            }
            
            audioSW.Stop();
            Logger.LogInfo($"Audio registered in {audioSW.ElapsedMilliseconds}ms");
        }

        private IEnumerator MakeSeaTreaderBlockersPassthrough()
        {
            yield return new WaitUntil(() => WaitScreen.main);
            
            CraftData.PreparePrefabIDCache();
            var task = PrefabDatabase.GetPrefabAsync("626f6739-acb0-4dfc-bbab-9b627767403c");
            yield return task;

            task.TryGetPrefab(out var prefab);
            prefab.EnsureComponent<DontCollideWithPlayer>();
        }

        private void LoadPathfindingGrid()
        {
            byte[] bytes = GeneralAssetBundle.LoadAsset<TextAsset>("SaveGrid.grid").bytes;
            ThreadStart threadStart = () => DeserializeGridData(bytes, saveData =>
            {
                pathfindingGridSaveData = saveData;
                onLoadGridSaveData?.Invoke(saveData);
            });

            var gridLoadThread = new Thread(threadStart);
            gridLoadThread.Start();
        }

        private void DeserializeGridData(byte[] bytes, Action<GridSaveData> callback)
        {
            var data = SaveManager.DeserializeObject<GridSaveData>(bytes);
            callback?.Invoke(data);
        }
    }
}
