using Nautilus.Handlers;
using Nautilus.Utility;
using PrototypeSubMod.Patches;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.Prefabs.FacilityProps.Hull;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.Registration;

internal static class EncyEntryRegisterer
{
    public static void Register()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        
        #region Prototype

        var protoPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("PrototypeSub_EncyPopup");
        string protoTitle = Language.main.Get("ProtoDatabankEncy_Title");
        string protoBody = Language.main.Get("ProtoDatabankEncy_Body");
        Texture2D prototypeBackground = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("PrototypeSubEncy");

        PDAHandler.AddEncyclopediaEntry("ProtoDatabankEncy", "DownloadedData/Prototype/ProtoTerminal", protoTitle, protoBody,
            prototypeBackground, protoPopup, PDAHandler.UnlockImportant);
        #endregion

        #region Precursor Ingot
        string ingotTitle = Language.main.Get("ProtoPrecursorIngotEncy_Title");
        string ingotDescription = Language.main.Get("ProtoPrecursorIngotEncy_Body");
        var ingotPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("AlienFramework_EncyPopup");

        PDAHandler.AddEncyclopediaEntry("ProtoPrecursorIngot", "DownloadedData/Prototype/Scanned", ingotTitle,
            ingotDescription, unlockSound: PDAHandler.UnlockBasic, popupImage: ingotPopup);
        var precursorIngotEntryData = new PDAScanner.EntryData()
        {
            key = PrecursorIngot_Craftable.prefabInfo.TechType,
            destroyAfterScan = false,
            encyclopedia = "ProtoPrecursorIngot",
            scanTime = 5f,
            isFragment = false,
            blueprint = PrecursorIngot_Craftable.prefabInfo.TechType
        };
        PDAHandler.AddCustomScannerEntry(precursorIngotEntryData);
        #endregion

        #region Deployable Light
        string lightTitle = Language.main.Get("ProtoDeployableLightEncy_Title");
        string lightDescription = Language.main.Get("ProtoDeployableLightEncy_Body");
        var lightPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("DeployableLight_EncyPopup");
        Texture2D lightBackground = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("PhotonBeaconEncy");
        
        PDAHandler.AddEncyclopediaEntry("ProtoDeployableLightEncy", "DownloadedData/Prototype/Scanned", lightTitle, lightDescription, image: lightBackground, unlockSound: PDAHandler.UnlockBasic, popupImage: lightPopup);
        var deployableLightEntryData = new PDAScanner.EntryData()
        {
            key = DeployableLight_Craftable.prefabInfo.TechType,
            destroyAfterScan = false,
            encyclopedia = "ProtoDeployableLightEncy",
            scanTime = 5f,
            isFragment = false,
            blueprint = DeployableLight_Craftable.prefabInfo.TechType
        };
        PDAHandler.AddCustomScannerEntry(deployableLightEntryData);
        #endregion

        #region Ion Prism
        string prismTitle = Language.main.Get("ProtoIonPrismEncy_Title");
        string prismDescription = Language.main.Get("ProtoIonPrismEncy_Body");
        var prismPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("IonPrism_EncyPopup");

        PDAHandler.AddEncyclopediaEntry("ProtoIonPrismEncy", "DownloadedData/Prototype/Scanned", prismTitle,
            prismDescription, unlockSound: PDAHandler.UnlockBasic, popupImage: prismPopup);
        var ionPrismEntryData = new PDAScanner.EntryData()
        {
            key = IonPrism_Craftable.prefabInfo.TechType,
            destroyAfterScan = false,
            encyclopedia = "ProtoIonPrismEncy",
            scanTime = 5f,
            isFragment = false,
            blueprint = IonPrism_Craftable.prefabInfo.TechType
        };
        PDAHandler.AddCustomScannerEntry(ionPrismEntryData);
        #endregion

        #region Orion Logs
        string orionTitle = Language.main.Get("OrionFacilityLogs_Title");
        string orionBody = Language.main.Get("OrionFacilityLogs_Body");
        PDAHandler.AddEncyclopediaEntry("OrionFacilityLogsEncy", "DownloadedData/Prototype/ProtoTerminal", orionTitle,
            orionBody, unlockSound: PDAHandler.UnlockBasic);
        #endregion

        #region Defense Audit Logs
        string defenseAuditTitle = Language.main.Get("DefenseFacilityLogs_Title");
        string defenseAuditBody = Language.main.Get("DefenseFacilityLogs_Body");
        PDAHandler.AddEncyclopediaEntry("DefenseFacilityAuditEncy", "DownloadedData/Prototype/ProtoTerminal",
            defenseAuditTitle, defenseAuditBody, unlockSound: PDAHandler.UnlockBasic);
        #endregion

        #region Engine Audit Logs
        string engineAuditTitle = Language.main.Get("EngineFacilityLogs_Title");
        string engineAuditBody = Language.main.Get("EngineFacilityLogs_Body");
        PDAHandler.AddEncyclopediaEntry("EngineFacilityAuditEncy", "DownloadedData/Prototype/ProtoTerminal",
            engineAuditTitle, engineAuditBody, unlockSound: PDAHandler.UnlockBasic);
        #endregion

        #region Facility Locations
        string locationsTitle = Language.main.Get("ProtoFacilitiesEncy_Title");
        string locationsBody = Language.main.Get("ProtoFacilitiesEncy_Body");
        PDAHandler.AddEncyclopediaEntry("ProtoFacilitiesEncy", "DownloadedData/Prototype/ProtoTerminal", locationsTitle,
            locationsBody, unlockSound: PDAHandler.UnlockBasic);
        #endregion

        #region Orion Fragmentor
        string fragmentorTitle = Language.main.Get("OrionFragmentorEncy_Title");
        string fragmentorBody = Language.main.Get("OrionFragmentorEncy_Body");
        PDAHandler.AddEncyclopediaEntry("OrionFragmentorEncy", "DownloadedData/Prototype/Scanned", fragmentorTitle,
            fragmentorBody, unlockSound: PDAHandler.UnlockBasic);
        
        var orionFragmentorEntryData = new PDAScanner.EntryData()
        {
            key = OrionFragmentor_World.prefabInfo.TechType,
            destroyAfterScan = false,
            encyclopedia = "OrionFragmentorEncy",
            scanTime = 10f,
            isFragment = false,
            blueprint = OrionFragmentor_World.prefabInfo.TechType
        };
        PDAHandler.AddCustomScannerEntry(orionFragmentorEntryData);
        #endregion
        
        #region Hull Facility Logs
        string hullFacilityLogsTitle = Language.main.Get("HullFacilityLogsEncy_Title");
        string hullFacilityLogsBody = Language.main.Get("HullFacilityLogsEncy_Body");
        PDAHandler.AddEncyclopediaEntry("HullFacilityLogsEncy", "DownloadedData/Prototype/ProtoTerminal",
            hullFacilityLogsTitle, hullFacilityLogsBody, unlockSound: PDAHandler.UnlockBasic);
        #endregion

        #region Orion Endeavors
        string orionEndeavorsTitle = Language.main.Get("OrionEndeavorsEncy_Title");
        string orionEndeavorsBody = Language.main.Get("OrionEndeavorsEncy_Body");
        PDAHandler.AddEncyclopediaEntry("OrionEndeavorsEncy", "DownloadedData/Prototype/ProtoTerminal", orionEndeavorsTitle,
            orionEndeavorsBody, unlockSound: PDAHandler.UnlockBasic);
        #endregion

        #region Alien Building Block
        string alienBuildingBlockEncyTitle = Language.main.Get("AlienBuildingBlockEncy_Title");
        string alienBuildingBlockEncyBody = Language.main.Get("AlienBuildingBlockEncy_Body");
        PDAHandler.AddEncyclopediaEntry("AlienBuildingBlockEncy", "DownloadedData/Prototype/ProtoTerminal",
            alienBuildingBlockEncyTitle, alienBuildingBlockEncyBody, unlockSound: PDAHandler.UnlockBasic);
        #endregion

        #region Hull Facility Tablet
        string hullTabletTitle = Language.main.Get("HullFacilityTabletEncy_Title");
        string hullTabletDescription = Language.main.Get("HullFacilityTabletEncy_Body");
        var hullTabletPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("HullFacilityTablet_EncyPopup");
        
        PDAHandler.AddEncyclopediaEntry("HullFacilityTabletEncy", "DownloadedData/Prototype/Scanned", hullTabletTitle, hullTabletDescription, unlockSound: PDAHandler.UnlockBasic, popupImage: hullTabletPopup);
        #endregion
        
        #region Engine Facility Tablet
        string engineTabletTitle = Language.main.Get("EngineFacilityTabletEncy_Title");
        string engineTabletDescription = Language.main.Get("EngineFacilityTabletEncy_Body");
        var engineTabletPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("EngineFacilityTablet_EncyPopup");
        
        PDAHandler.AddEncyclopediaEntry("EngineFacilityTabletEncy", "DownloadedData/Prototype/Scanned", engineTabletTitle, engineTabletDescription, unlockSound: PDAHandler.UnlockBasic, popupImage: engineTabletPopup);
        #endregion
        
        #region Interceptor Facility Tablet
        string interceptorTabletTitle = Language.main.Get("InterceptorFacilityTabletEncy_Title");
        string interceptorTabletDescription = Language.main.Get("InterceptorFacilityTabletEncy_Body");
        var interceptorTabletPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("InterceptorFacilityTablet_EncyPopup");
        
        PDAHandler.AddEncyclopediaEntry("InterceptorFacilityTabletEncy", "DownloadedData/Prototype/Scanned", interceptorTabletTitle, interceptorTabletDescription, unlockSound: PDAHandler.UnlockBasic, popupImage: interceptorTabletPopup);
        #endregion
        
        #region Defense Facility Tablet
        string defenseTabletTitle = Language.main.Get("DefenseFacilityTabletEncy_Title");
        string defenseTabletDescription = Language.main.Get("DefenseFacilityTabletEncy_Body");
        var defenseTabletPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("DefenseFacilityTablet_EncyPopup");
        
        PDAHandler.AddEncyclopediaEntry("DefenseFacilityTabletEncy", "DownloadedData/Prototype/Scanned", defenseTabletTitle, defenseTabletDescription, unlockSound: PDAHandler.UnlockBasic, popupImage: defenseTabletPopup);
        #endregion

        #region Decorative Worm
        TechType decorativeWormType = (TechType)Enum.Parse(typeof(TechType), "ProtoDecorativeWorm");
        string decorativeWormTitle = Language.main.Get("ProtoDecorativeWormEncy_Title");
        string decorativeWormDescription = Language.main.Get("ProtoDecorativeWormEncy_Body");
        
        PDAHandler.AddEncyclopediaEntry("ProtoDecorativeWormEncy", "DownloadedData/Prototype/Scanned", decorativeWormTitle, 
            decorativeWormDescription, unlockSound: PDAHandler.UnlockBasic);
        var decorativeWormEntryData = new PDAScanner.EntryData()
        {
            key = decorativeWormType,
            destroyAfterScan = false,
            encyclopedia = "ProtoDecorativeWormEncy",
            scanTime = 5f,
            isFragment = false,
            blueprint = decorativeWormType
        };
        PDAHandler.AddCustomScannerEntry(decorativeWormEntryData);
        PDAEncyclopedia_Patches.EncyclopediaUnlockEvents.Add("ProtoDecorativeWormEncy", () =>
        {
            PDALog.Add("OnScanDisabledWyrm");
        });
        #endregion
        
        #region Hanging Worm
        TechType hangingWormType = (TechType)Enum.Parse(typeof(TechType), "ProtoHangingWorm");
        string hangingWormTitle = Language.main.Get("ProtoHangingWormEncy_Title");
        string hangingWormDescription = Language.main.Get("ProtoHangingWormEncy_Body");
        
        PDAHandler.AddEncyclopediaEntry("ProtoHangingWormEncy", "DownloadedData/Precursor/Scan", hangingWormTitle, 
            hangingWormDescription, unlockSound: PDAHandler.UnlockBasic);
        var hangingWormEntryData = new PDAScanner.EntryData()
        {
            key = hangingWormType,
            destroyAfterScan = false,
            encyclopedia = "ProtoHangingWormEncy",
            scanTime = 5f,
            isFragment = false,
            blueprint = hangingWormType
        };
        PDAHandler.AddCustomScannerEntry(hangingWormEntryData);
        #endregion
        
        #region Normal Worm
        TechType normalWormType = (TechType)Enum.Parse(typeof(TechType), "ProtoWorm");
        string normalWormTitle = Language.main.Get("ProtoWormEncy_Title");
        string normalWormDescription = Language.main.Get("ProtoWormEncy_Body");
        Texture2D normalWormBackground = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("ProtoWormEncy");
        var wormPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("Wyrm_EncyPopup");
        
        PDAHandler.AddEncyclopediaEntry("ProtoWormEncy", "DownloadedData/Prototype/Scanned", normalWormTitle, 
            normalWormDescription, normalWormBackground, wormPopup, PDAHandler.UnlockBasic);
        var normalWormEntryData = new PDAScanner.EntryData()
        {
            key = normalWormType,
            destroyAfterScan = false,
            encyclopedia = "ProtoWormEncy",
            scanTime = 5f,
            isFragment = false,
            blueprint = normalWormType
        };
        PDAHandler.AddCustomScannerEntry(normalWormEntryData);
        #endregion

        #region Aggressive Wyrm
        TechType aggressiveWyrmType = (TechType)Enum.Parse(typeof(TechType), "ProtoAggressiveWyrm");
        string aggressiveWyrmTitle = Language.main.Get("ProtoAggressiveWyrmEncy_Title");
        string aggressiveWyrmDescription = Language.main.Get("ProtoAggressiveWyrmEncy_Body");
        Texture2D aggressiveWyrmBackground = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("ProtoWyrmEncy");
        var aggressiveWyrmPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("Wyrm_EncyPopup");
        
        PDAHandler.AddEncyclopediaEntry("ProtoAggressiveWyrmEncy", "DownloadedData/Prototype/Scanned", aggressiveWyrmTitle,
            aggressiveWyrmDescription, aggressiveWyrmBackground, aggressiveWyrmPopup, PDAHandler.UnlockBasic);
        var normalWyrmEntryData = new PDAScanner.EntryData()
        {
            key = aggressiveWyrmType,
            destroyAfterScan = false,
            encyclopedia = "ProtoAggressiveWyrmEncy",
            scanTime = 5f,
            isFragment = false,
            blueprint = aggressiveWyrmType
        };
        PDAHandler.AddCustomScannerEntry(normalWyrmEntryData);
        #endregion

        #region Warp Core
        var image = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("WarpReactorEncy");
        var warpPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("WarpReactor_EncyPopup");
        TechType warpCoreType = (TechType)Enum.Parse(typeof(TechType), "WarpReactor");
        string warpReactorTitle = Language.main.Get("ProtoWarpReactorEncy_Title");
        string warpReactorBody = Language.main.Get("ProtoWarpReactorEncy_Body");
        
        PDAHandler.AddEncyclopediaEntry("ProtoWarpReactorEncy", "DownloadedData/Prototype/Scanned", warpReactorTitle, 
            warpReactorBody, image, warpPopup, PDAHandler.UnlockBasic);
        var warpCoreEntryData = new PDAScanner.EntryData()
        {
            key = warpCoreType,
            destroyAfterScan = false,
            encyclopedia = "ProtoWarpReactorEncy",
            scanTime = 8f,
            isFragment = false,
            blueprint = warpCoreType
        };
        PDAHandler.AddCustomScannerEntry(warpCoreEntryData);
        #endregion

        #region Dead Zone Mapping Initiative Project Data
        string wormTerminalTitle = Language.main.Get("HullFacilityWormTerminalEncy_Title");
        string wormTerminalDescription = Language.main.Get("HullFacilityWormTerminalEncy_Body");

        PDAHandler.AddEncyclopediaEntry("HullFacilityWormTerminalEncy", "DownloadedData/Prototype/ProtoTerminal", wormTerminalTitle, wormTerminalDescription, unlockSound: PDAHandler.UnlockBasic);
        #endregion

        #region Fragmentation Terminal
        string fragmentationTerminalTitle = Language.main.Get("FragmentationTerminalEncy_Title");
        string fragmentationTerminalDescription = Language.main.Get("FragmentationTerminalEncy_Body");

        PDAHandler.AddEncyclopediaEntry("FragmentationTerminalEncy", "DownloadedData/Prototype/ProtoTerminal", fragmentationTerminalTitle, fragmentationTerminalDescription, unlockSound: PDAHandler.UnlockBasic);
        #endregion

        #region Animate Entropy Terminal
        string animateEntropyTerminalTitle = Language.main.Get("AnimateEntropyTerminalEncy_Title");
        string animateEntropyTerminalDescription = Language.main.Get("AnimateEntropyTerminalEncy_Body");

        PDAHandler.AddEncyclopediaEntry("AnimateEntropyTerminalEncy", "DownloadedData/Prototype/ProtoTerminal", animateEntropyTerminalTitle, animateEntropyTerminalDescription, unlockSound: PDAHandler.UnlockBasic);
        #endregion

        #region Prototype Fins
        Texture2D finsBackground = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("ProtoFinsEncy");
        TechType finsType = (TechType)Enum.Parse(typeof(TechType), "ProtoScannableFins");
        string finsTitle = Language.main.Get("ProtoFins_Title");
        string finsBody = Language.main.Get("ProtoFins_Body");
        var finsPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("ProtoFins_EncyPopup");

        PDAHandler.AddEncyclopediaEntry("ProtoFinsEncy", "DownloadedData/Prototype/Scanned", finsTitle, 
            finsBody, image: finsBackground, unlockSound: PDAHandler.UnlockBasic, popupImage: finsPopup);
        var protoFinsEntry = new PDAScanner.EntryData()
        {
            key = finsType,
            destroyAfterScan = false,
            encyclopedia = "ProtoFinsEncy",
            scanTime = 4f,
            isFragment = false,
            blueprint = finsType
        };
        PDAHandler.AddCustomScannerEntry(protoFinsEntry);
        #endregion
        
        #region Build Terminal
        string terminalTitle = Language.main.Get("ProtoBuildTerminal_Title");
        string terminalBody = Language.main.Get("ProtoBuildTerminal_Body");
        
        PDAHandler.AddEncyclopediaEntry("ProtoBuildTerminalEncy", "DownloadedData/Prototype/Scanned", terminalTitle, 
            terminalBody, unlockSound: PDAHandler.UnlockBasic);
        var buildTerminalType = (TechType)Enum.Parse(typeof(TechType), "ProtoBuildTerminal");
        var terminalEntry = new PDAScanner.EntryData()
        {
            key = buildTerminalType,
            destroyAfterScan = false,
            encyclopedia = "ProtoBuildTerminalEncy",
            scanTime = 8f,
            isFragment = false,
            blueprint = buildTerminalType
        };
        PDAHandler.AddCustomScannerEntry(terminalEntry);
        #endregion
        
        #region Defense wall

        var techType = (TechType)Enum.Parse(typeof(TechType), "DefenseFacilityWall");
        string wallTitle = Language.main.Get("DefenseFacilityWallEncy_Title");
        string wallBody = Language.main.Get("DefenseFacilityWallEncy_Body");
        
        PDAHandler.AddEncyclopediaEntry("DefenseFacilityWallEncy", "DownloadedData/Prototype/Scanned", wallTitle, 
            wallBody, unlockSound: PDAHandler.UnlockBasic);
        var wallEntry = new PDAScanner.EntryData()
        {
            key = techType,
            destroyAfterScan = false,
            encyclopedia = "DefenseFacilityWallEncy",
            scanTime = 10f,
            isFragment = false,
            blueprint = techType
        };
        PDAHandler.AddCustomScannerEntry(wallEntry);
        #endregion

        #region Transmission Device Terminal
        string transmissionTerminalTitle = Language.main.Get("TransmissionTerminalEncy_Title");
        string transmissionTerminalDescription = Language.main.Get("TransmissionTerminalEncy_Body");
        var transmissionDevicePopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("TransmissionDevice_Icon");

        PDAHandler.AddEncyclopediaEntry("TransmissionTerminalEncy", "DownloadedData/Prototype/ProtoTerminal", transmissionTerminalTitle, transmissionTerminalDescription, unlockSound: PDAHandler.UnlockBasic, popupImage: transmissionDevicePopup);
        #endregion

        #region Precursor Suit Terminal
        string precursorSuitTerminalTitle = Language.main.Get("PrecursorSuitTerminalEncy_Title");
        string precursorSuitTerminalDescription = Language.main.Get("PrecursorSuitTerminalEncy_Body");
        var precursorSuitPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("PrecursorSuit_EncyPopup");

        PDAHandler.AddEncyclopediaEntry("PrecursorSuitTerminalEncy", "DownloadedData/Prototype/ProtoTerminal", precursorSuitTerminalTitle, precursorSuitTerminalDescription, unlockSound: PDAHandler.UnlockBasic, popupImage: precursorSuitPopup);
        #endregion

        #region Biomechanics Factor Terminal
        string biomechanicsFactorTerminalTitle = Language.main.Get("BiomechanicsFactorTerminalEncy_Title");
        string biomechanicsFactorTerminalDescription = Language.main.Get("BiomechanicsFactorTerminalEncy_Body");
        var biomechanicsFactorPopup = Plugin.GeneralAssetBundle.LoadAsset<Sprite>("Factor_EncyPopup");

        PDAHandler.AddEncyclopediaEntry("BiomechanicsFactorTerminalEncy", "DownloadedData/Prototype/ProtoTerminal", biomechanicsFactorTerminalTitle, biomechanicsFactorTerminalDescription, unlockSound: PDAHandler.UnlockBasic, popupImage: biomechanicsFactorPopup);
        #endregion

        #region Transmission Site Hint

        var transmissionSiteTitle = Language.main.Get("ProtoTransmissionSiteEncy_Title");
        var transmissionSiteBody = Language.main.Get("ProtoTransmissionSiteEncy_Body");

        PDAHandler.AddEncyclopediaEntry("ProtoTransmissionSiteEncy", "DownloadedData/Prototype", transmissionSiteTitle,
            transmissionSiteBody, unlockSound: PDAHandler.UnlockBasic);

        #endregion
        
        #region Number Puzzle PDA

        var numberPuzzlePDATitle = Language.main.Get("NumberPuzzlePDAEncy_Title");
        var numberPuzzlePDABody = Language.main.Get("NumberPuzzlePDAEncy_Body");

        PDAHandler.AddEncyclopediaEntry("NumberPuzzlePDAEncy", "DownloadedData/AuroraSurvivors", numberPuzzlePDATitle,
            numberPuzzlePDABody, unlockSound: PDAHandler.UnlockImportant, voiceLog: AudioUtils.GetFmodAsset("PDA_NumberPuzzleSurvivor"));
        #endregion
        
        #region Puzzle Numbers
        
        string protoNumbersTitle = Language.main.Get("ProtoNumbersEncy_Title");
        string protoNumbersDescription = Language.main.Get("ProtoNumbersEncy_Body");
        Texture2D protoNumbersBackground = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("ProtoNumbersEncy");
        
        PDAHandler.AddEncyclopediaEntry("ProtoNumbersEncy", "DownloadedData/Prototype/ProtoTerminal", protoNumbersTitle,
            protoNumbersDescription, protoNumbersBackground, unlockSound: PDAHandler.UnlockBasic);
        #endregion

        RegisterEncyEntries("DownloadedData/Prototype/ProtoUpgrades", PDAHandler.UnlockBasic, new()
        {
            "ProtoCloakEncy",
            "ProtoEmergencyWarpEncy",
            "ProtoInterceptorEncy",
            "ProtoRepairDroidsEncy",
            "ProtoDepthOptimizersEncy",
            "ProtoIonBarrierEncy",
            "ProtoStasisPulseEncy",
            "ProtoIonGeneratorEncy",
            "ProtoOverclockEncy"
        });

        sw.Stop();
        Plugin.Logger.LogInfo($"Ency entries registered in {sw.ElapsedMilliseconds}ms");
    }

    private static void RegisterEncyEntries(string path, FMODAsset unlockSound, List<string> entries)
    {
        foreach (var entry in entries)
        {
            string title = Language.main.Get($"{entry}_Title");
            string body = Language.main.Get($"{entry}_Body");
            PDAHandler.AddEncyclopediaEntry(entry, path, title, body, unlockSound: unlockSound);
        }
    }
}
