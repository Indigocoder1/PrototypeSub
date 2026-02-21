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

        #region Biomechanics Factor Unlock
        StoryGoalHandler.RegisterCustomEvent("BiomechanicsFactorTerminal", () =>
        {
            KnownTech.Add(BiomechanicsFactor.prefabInfo.TechType);
            PDAEncyclopedia.Add("BiomechanicsFactorTerminalEncy", true);
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

        #region Number Puzzle PDA
        StoryGoalHandler.RegisterCustomEvent("ProtoLifepod3PDA", () =>
        {
            PDAEncyclopedia.Add("NumberPuzzlePDAEncy", true);
        });
        #endregion

        #region Number Puzzle Entry Voiceline
        StoryGoalHandler.RegisterCustomEvent("OnEnterProtoNumberPuzzle", () =>
        {
            PDALog.Add("OnEnterProtoNumberPuzzle");
        });
        #endregion
        
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
        
        StoryGoalHandler.RegisterCompoundGoal("Ency_ProtoFacilitiesEncy", Story.GoalType.Story, 306f,
            "PrototypeCrafted");
        StoryGoalHandler.RegisterCompoundGoal("ProtoFacilityLocationsHint", Story.GoalType.Story, 300f,
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
