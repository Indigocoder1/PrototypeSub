using System;
using Nautilus.Handlers;
using Nautilus.Utility;
using PrototypeSubMod.Facilities.Hull;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.Prefabs.FacilityProps;
using PrototypeSubMod.Prefabs.Factors;
using PrototypeSubMod.PrototypeStory;
using UnityEngine;

namespace PrototypeSubMod.Registration;

internal static class StoryGoalsRegisterer
{
    public static void Register()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        
        #region Precursor Ingot

        StoryGoalHandler.RegisterItemGoal("Ency_ProtoPrecursorIngot", Story.GoalType.Encyclopedia, PrecursorIngot_Craftable.prefabInfo.TechType);

        StoryGoalHandler.RegisterCustomEvent("Ency_ProtoPrecursorIngot", () =>
        {
            KnownTech.Add(PrecursorIngot_Craftable.prefabInfo.TechType);
            PDAEncyclopedia.Add("ProtoPrecursorIngot", true);
        });
        #endregion

        #region Phase Gate Items Pickup
        StoryGoalHandler.RegisterItemGoal("Ency_ProtoPhaseGateStructure", Story.GoalType.Encyclopedia, ProtoPhaseGateStructure.PrefabInfo.TechType);

        StoryGoalHandler.RegisterCustomEvent("Ency_ProtoPhaseGateStructure", () =>
        {
            KnownTech.Add(ProtoPhaseGateStructure.PrefabInfo.TechType);
            PDAEncyclopedia.Add("ProtoPhaseGateStructure", true);
        });

        StoryGoalHandler.RegisterItemGoal("Ency_ProtoPhaseGateStabilizer", Story.GoalType.Encyclopedia, ProtoPhaseGateStabilizer.PrefabInfo.TechType);

        StoryGoalHandler.RegisterCustomEvent("Ency_ProtoPhaseGateStabilizer", () =>
        {
            KnownTech.Add(ProtoPhaseGateStabilizer.PrefabInfo.TechType);
            PDAEncyclopedia.Add("ProtoPhaseGateStabilizer", true);
        });

        StoryGoalHandler.RegisterItemGoal("Ency_ProtoPhaseGateTransmitter", Story.GoalType.Encyclopedia, ProtoPhaseGateTransmitter.PrefabInfo.TechType);

        StoryGoalHandler.RegisterCustomEvent("Ency_ProtoPhaseGateTransmitter", () =>
        {
            KnownTech.Add(ProtoPhaseGateTransmitter.PrefabInfo.TechType);
            PDAEncyclopedia.Add("ProtoPhaseGateTransmitter", true);
        });
        #endregion

        #region PPT First Interaction
        PPTStoryManager.RegisterGoals();
        #endregion

        #region Interceptor Unlock
        StoryGoalHandler.RegisterCustomEvent("OnInterceptorTestDataDownloaded", () =>
        {
            PDALog.Add("OnInterceptorTestDataDownloaded");
        });

        StoryGoalHandler.RegisterCompoundGoal("InterceptorTestEncy", Story.GoalType.Encyclopedia, 15f, new[] { "OnInterceptorTestDataDownloaded" });
        StoryGoalHandler.RegisterCustomEvent("InterceptorTestEncy", () =>
        {
            PDAEncyclopedia.Add("InterceptorTestEncy", true);
        });
        #endregion

        #region Disable Defense Cloak
        StoryGoalHandler.RegisterCustomEvent("OnDefenseCloakDisabled", () =>
        {
            PDALog.Add("OnDefenseCloakDisabled");
            FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("EngineAllBreachesRepaired"), Player.main.transform.position);
        });
        #endregion

        #region Moonpool Enter
        StoryGoalHandler.RegisterCustomEvent("OnEnterDefenseMoonpool", () =>
        {
            PDALog.Add("OnEnterDefenseMoonpool");
        });

        StoryGoalHandler.RegisterLocationGoal("OnEnterDefenseMoonpool", Story.GoalType.PDA, new Vector3(819, -463, -1115), 15, 0);
        #endregion

        #region Moonpool Open Disallowed
        StoryGoalHandler.RegisterCustomEvent("OnMoonpoolNoPrototype", () =>
        {
            PDALog.Add("OnMoonpoolNoPrototype");
        });
        #endregion

        #region On Approach Defense Beacon
        StoryGoalHandler.RegisterCustomEvent("OnApproachDefenseFacility", () =>
        {
            PDALog.Add("OnApproachDefenseFacility");
        });
        #endregion

        #region Orion Logs
        StoryGoalHandler.RegisterCustomEvent("Ency_OrionFacilityLogs", () =>
        {
            PDAEncyclopedia.Add("OrionFacilityLogsEncy", true);
        });
        #endregion

        #region Facility Locations
        StoryGoalHandler.RegisterCustomEvent("Ency_ProtoFacilitiesEncy", () =>
        {
            PDAEncyclopedia.Add("ProtoFacilitiesEncy", true);
        });
        StoryGoalHandler.RegisterCustomEvent("ProtoFacilityLocationsHint", () =>
        {
            PDALog.Add("ProtoFacilityLocationsHint", true);
        });
        #endregion

        #region Defense Audit Logs
        StoryGoalHandler.RegisterCompoundGoal("DefenseFacilityAuditEncy", Story.GoalType.Encyclopedia, 7f, "OnDisableDefenseCloak");

        StoryGoalHandler.RegisterCustomEvent("DefenseFacilityAuditEncy", () =>
        {
            PDAEncyclopedia.Add("DefenseFacilityAuditEncy", true);
            
            KnownTech.Add(DefenseFacilityKey.prefabInfo.TechType);

            PDAEncyclopedia.Add("DefenseFacilityKey", true);
        });
        #endregion

        #region Engine Audit Logs
        StoryGoalHandler.RegisterCustomEvent("EngineFacilityAuditEncy", () =>
        {
            PDAEncyclopedia.Add("EngineFacilityAuditEncy", true);
        });
        #endregion

        #region Enter Sub First Time

        StoryGoalHandler.RegisterCustomEvent("OnEnterSubFirstTime", null);

        #endregion

        #region Hull Facility Logs
        StoryGoalHandler.RegisterCustomEvent("HullFacilityLogsEncy", () =>
        {
            PDAEncyclopedia.Add("HullFacilityLogsEncy", true);
        });
        #endregion
        
        #region Hull Facility Orion Data
        StoryGoalHandler.RegisterCustomEvent("OrionEndeavorsEncy", () =>
        {
            PDAEncyclopedia.Add("OrionEndeavorsEncy", true);
        });
        #endregion

        #region Alien Building Block Info
        StoryGoalHandler.RegisterCustomEvent("AlienBuildingBlockEncy", () =>
        {
            PDAEncyclopedia.Add("AlienBuildingBlockEncy", true);
        });
        #endregion
        
        #region On Enter Engine Facility
        StoryGoalHandler.RegisterCustomEvent("OnEnterEngineFacility", () =>
        {
            PDALog.Add("OnEnterEngineFacility");
        });
        #endregion

        #region Dead Zone Mapping Initiative Project Data
        StoryGoalHandler.RegisterCustomEvent("HullFacilityWormTerminalEncy", () =>
        {
            PDAEncyclopedia.Add("HullFacilityWormTerminalEncy", true);
        });
        #endregion

        #region Fragmentation Terminal
        StoryGoalHandler.RegisterCustomEvent("FragmentationTerminalEncy", () =>
        {
            PDAEncyclopedia.Add("FragmentationTerminalEncy", true);
        });
        #endregion

        #region Animate Entropy Terminal
        StoryGoalHandler.RegisterCustomEvent("AnimateEntropyTerminalEncy", () =>
        {
            PDAEncyclopedia.Add("AnimateEntropyTerminalEncy", true);
        });
        #endregion

        #region Interceptor Facility Locked
        StoryGoalHandler.RegisterLocationGoal("OnApproachInterceptorFacility", Story.GoalType.Story,
            new Vector3(547, -709, 955), 400, 1);
        StoryGoalHandler.RegisterCustomEvent("OnApproachInterceptorFacility", () =>
        {
            if (!Plugin.GlobalSaveData.EngineFacilityPointsRepaired)
            {
                PDALog.Add("ProtoRevisitInterceptorFacility");
            }
        });
        #endregion

        #region Transmission Device Unlock
        StoryGoalHandler.RegisterCustomEvent("TransmissionDeviceUnlock", () =>
        {
            KnownTech.Add(ProtoTransmissionDevice.prefabInfo.TechType);
            PDAEncyclopedia.Add("TransmissionTerminalEncy", true);
        });
        #endregion

        #region Precursor Suit Unlock
        StoryGoalHandler.RegisterCustomEvent("PrecursorSuitTerminal", () =>
        {
            KnownTech.Add(PrecursorSuit.prefabInfo.TechType);
            PDAEncyclopedia.Add("PrecursorSuitTerminalEncy", true);
        });
        #endregion

        #region Tether Factor Unlock
        StoryGoalHandler.RegisterCustomEvent("TetherFactorTerminal", () =>
        {
            KnownTech.Add(TetherFactor.prefabInfo.TechType);
            PDAEncyclopedia.Add("TetherFactorEncy", true);
        });
        #endregion

        #region Biomechanics Factor Unlock
        StoryGoalHandler.RegisterCustomEvent("BiomechanicsFactorTerminal", () =>
        {
            KnownTech.Add(BiomechanicsFactor.prefabInfo.TechType);
            PDAEncyclopedia.Add("BiomechanicsFactorTerminalEncy", true);
        });
        #endregion
        
        #region Color Factor Unlock
        StoryGoalHandler.RegisterCustomEvent("ColorFactorTerminal", () =>
        {
            KnownTech.Add(SuitColorFactor.prefabInfo.TechType);
            PDAEncyclopedia.Add("SuitColorFactorEncy", true);
        });
        #endregion

        #region Propulsion Gloves Terminal
        StoryGoalHandler.RegisterCustomEvent("PrecursorPropulsionGlovesTerminal", () =>
        {
            KnownTech.Add(PrecursorPropulsionGloves.PrefabInfo.TechType);
            PDAEncyclopedia.Add("PrecursorPropulsionGlovesTerminalEncy", true);
        });
        #endregion

        #region Precursor Suit Pickup
        StoryGoalHandler.RegisterItemGoal("OnPrecursorSuitPickup", Story.GoalType.PDA, PrecursorSuit.prefabInfo.TechType);

        #endregion

        #region Hoverfish Plush Unlock
        StoryGoalHandler.RegisterCustomEvent("OnHoverfishPlushUnlocked", () =>
        {
            KnownTech.Add(HoverfishPlush.prefabInfo.TechType);
            PDALog.Add("OnHoverfishPlushUnlocked");
        });
        #endregion

        #region Survivor PDA 1
        StoryGoalHandler.RegisterCustomEvent("SurvivorPDA1", () =>
        {
            PDAEncyclopedia.Add("SurvivorPDA1Ency", true);
        });
        #endregion

        #region Survivor PDA 2
        StoryGoalHandler.RegisterCustomEvent("SurvivorPDA2", () =>
        {
            PDAEncyclopedia.Add("SurvivorPDA2Ency", true);
        });
        #endregion

        #region Number Puzzle Entry Voiceline
        StoryGoalHandler.RegisterCustomEvent("OnEnterProtoNumberPuzzle", () =>
        {
            PDALog.Add("OnEnterProtoNumberPuzzle");
        });
        #endregion

        #region Bearing Puzzle Entry Voiceline
        StoryGoalHandler.RegisterCustomEvent("OnEnterProtoBearingPuzzle", () =>
        {
            PDALog.Add("OnEnterProtoBearingPuzzle");
        });
        #endregion

        #region Lifepod 3 PDA
        StoryGoalHandler.RegisterCustomEvent("ProtoLifepod3PDA", () =>
        {
            PDAEncyclopedia.Add("Lifepod3PDAEncy", true);
        });
        #endregion

        #region Number Puzzle Completion
        StoryGoalHandler.RegisterCustomEvent("ProtoNumberPuzzleComplete", () =>
        {

        });

        StoryGoalHandler.RegisterCustomEvent("Ency_ProtoNumbers", () =>
        {
            PDAEncyclopedia.Add("ProtoNumbersEncy", true);
        });
        StoryGoalHandler.RegisterCustomEvent("ProtoNumbersHint", () =>
        {
            PDALog.Add("ProtoNumbersHint", true);
        });

        StoryGoalHandler.RegisterCompoundGoal("Ency_ProtoNumbers", Story.GoalType.Story, 15f,
            "ProtoNumberPuzzleComplete");
        StoryGoalHandler.RegisterCompoundGoal("ProtoNumbersHint", Story.GoalType.Story, 10f,
            "ProtoNumberPuzzleComplete");
        #endregion

        #region Calibration Site Completion
        StoryGoalHandler.RegisterCustomEvent("OnCalibrationRunCompleted", () =>
        {
        });

        StoryGoalHandler.RegisterCustomEvent("ProtoCalibrationCodeEncy", () =>
        {
            PDALog.Add("Proto_OnCalibrationRunCompleted");
            PDAEncyclopedia.Add("ProtoCalibrationCodeEncy", true);
        });

        StoryGoalHandler.RegisterCompoundGoal("ProtoCalibrationCodeEncy", Story.GoalType.Story, 15f,
            "OnCalibrationRunCompleted");

        #endregion

        #region On Transmission Site Reached
        StoryGoalHandler.RegisterBiomeGoal("OnTransmissionSiteReached", Story.GoalType.Story,
            BiomeRegisterer.TransmissionSiteBiome, 10f);
        StoryGoalHandler.RegisterCustomEvent("OnTransmissionSiteReached", () =>
        {
            PDALog.Add("Proto_OnTransmissionSiteReached");
        });
        #endregion

        #region Transmission Device First Loaded
        StoryGoalHandler.RegisterCustomEvent("TransmissionDeviceFirstLoaded", () =>
        {
            PDALog.Add("Proto_OnTransmissionDeviceFirstLoaded");
        });
        #endregion

        #region Engine Facility Scream + PDA hint
        StoryGoalHandler.RegisterCustomEvent("EngineScream", () =>
        {
            FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("EngineScream"), new Vector3(-1000, -400, -1100));
        });

        StoryGoalHandler.RegisterCustomEvent("EngineFacilityReturnHint", () =>
        {
            PDALog.Add("EngineFacilityReturnHint");
        });

        StoryGoalHandler.RegisterCompoundGoal("EngineFacilityReturnHint", Story.GoalType.PDA, 120f, "OrionSurgicalRoomTome");
        StoryGoalHandler.RegisterCompoundGoal("EngineScream", Story.GoalType.Story, 7f, "EngineFacilityReturnHint");
        #endregion

        StoryGoalHandler.RegisterCustomEvent("HullFacilityTeleporterUnlocked", () =>
        {
            FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("EngineAllBreachesRepaired"), Player.main.transform.position);
        });

        StoryGoalHandler.RegisterCustomEvent("OrionSurgicalRoomTome", () =>
        {
            FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("HullFacilityOrionTone"), Player.main.transform.position);
        });

        StoryGoalHandler.RegisterCustomEvent("HullFacilityActivateWorm", () => WormSpawnEvent.TimeWormsEnabled = Time.time);
        StoryGoalHandler.RegisterCustomEvent("PrototypeCrafted", () =>
        {
            var finType1 = (TechType)Enum.Parse(typeof(TechType), "ProtoFinUpgrade1");
            KnownTech.Add(finType1);
            
            var relayType1 = (TechType)Enum.Parse(typeof(TechType), "ProtoRelayUpgrade1");
            KnownTech.Add(relayType1);
        });

        StoryGoalHandler.RegisterCustomEvent("LocatorFactorTerminal", () =>
        {
            KnownTech.Add(LocatorFactor.prefabInfo.TechType);
            PDAEncyclopedia.Add("LocatorFactorTerminalEncy", true);
        });
        
        StoryGoalHandler.RegisterCompoundGoal("Ency_ProtoFacilitiesEncy", Story.GoalType.Story, 156,
            "PrototypeCrafted");
        StoryGoalHandler.RegisterCompoundGoal("ProtoFacilityLocationsHint", Story.GoalType.Story, 150,
            "PrototypeCrafted");

        StoryGoalHandler.RegisterItemGoal("OnPickupDefenseTablet", Story.GoalType.Story,
            DefenseFacilityKey.prefabInfo.TechType);
        StoryGoalHandler.RegisterCustomEvent("OnPickupDefenseTablet", () =>
        {
            KnownTech.Add(DefenseFacilityKey.prefabInfo.TechType);
            PDAEncyclopedia.Add("DefenseFacilityTabletEncy", true);
        });

        StoryGoalHandler.RegisterLocationGoal("ProtoApproachEngineFacility", Story.GoalType.Story, new Vector3(-530, -465, 1530), 300,
            3);
        StoryGoalHandler.RegisterCustomEvent("ProtoApproachEngineFacility", () =>
        {
            PDALog.Add("ProtoApproachEngineFacility");
        });

        StoryGoalHandler.RegisterCompoundGoal("UnlockEngineFacilityKey", Story.GoalType.Story, 16,
            "ProtoApproachEngineFacility");
        StoryGoalHandler.RegisterCustomEvent("UnlockEngineFacilityKey", () =>
        {
            KnownTech.Add(EngineFacilityKey.prefabInfo.TechType);
            PDAEncyclopedia.Add("EngineFacilityTabletEncy", true);
        });
        
        StoryGoalHandler.RegisterCustomEvent("OnEnterStoryEndProximity", () =>
        {
            PDALog.Add("OnEnterStoryEndProximity");
        });

        StoryGoalHandler.RegisterBiomeGoal("OnEnterPrecursorGun", Story.GoalType.PDA, "Precursor_Gun_OuterRooms", 0, delay: 20);
        StoryGoalHandler.RegisterCustomEvent("OnEnterPrecursorGun", () =>
        {
            PDALog.Add("OnEnterPrecursorGun");
        });
        
        StoryGoalHandler.RegisterCustomEvent("OnApproachPPT", () =>
        {
            PDALog.Add("Proto_ApproachTerminal");
        });
        
        StoryGoalHandler.RegisterBiomeGoal("ProtoOnEnterGrandReef", Story.GoalType.PDA, "grandReef", 10);
        StoryGoalHandler.RegisterCustomEvent("ProtoOnEnterGrandReef", () =>
        {
            PDALog.Add("ProtoOnEnterGrandReef");
        });
        
        sw.Stop();
        Plugin.Logger.LogInfo($"Story goals registered in {sw.ElapsedMilliseconds}ms");
    }
}
