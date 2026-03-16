using System.Collections.Generic;
using Nautilus.Handlers;

namespace PrototypeSubMod.Registration;

public static class VoicelineRegisterer
{
    private static readonly string[] VoicelineKeys = new[]
    {
        "Proto_AbandonShip",
        "Proto_AheadFlank",
        "Proto_AheadSlow",
        "Proto_AheadStandard",
        "Proto_CreatureAttack",
        "Proto_DeployingDecoy",
        "Proto_PhotonBeacon",
        "Proto_EmergencyPower",
        "Proto_EmergencyWarp",
        "Proto_HullCritical",
        "Proto_HullLow",
        "Proto_LDFActivate",
        "Proto_MaxDepth",
        "Proto_PoweringDown",
        "Proto_PoweringUp",
        "Proto_ProtocolThree",
        "Proto_ShieldsRaised",
        "Proto_TotalHullFailure",
        "Proto_OpenMoonpool",
        "Proto_WelcomeAboard",
        "Proto_WelcomeAboard_Issue",
        "Proto_WelcomeAboard_Critical",
        "Proto_HullDamage",
        "Proto_HullRestored",
        "Proto_CavitationDetected",
        "Proto_FireExtinguished",
        "Proto_FireDetected",
        "Proto_EngineOverheat",
        "Proto_EngineCritical",
        "Proto_OverclockEnabled",
        "Proto_OverrideTime1",
        "Proto_OverrideTime2",
        "Proto_NoPowerForWarp",
        "Proto_IonGeneratorActivate",
        "Proto_PowerSourceMax",
        "Proto_PowerSourceReplenished",
        "Proto_InterceptorLocked",
        "Proto_ReactorLocked",
        "Proto_InvalidOperation",
        "Proto_InsufficientPower",
        "Proto_LowPower",
        "Proto_CriticalPower",
        "Proto_FireSuppression",
        "Proto_OnTransmissionSiteReached",
        
        "Proto_AlphaDeployed",
        "Proto_BetaDeployed",
        "Proto_AlphaOnline",
        "Proto_BetaOnline",
        "Proto_AlphaOffline",
        "Proto_BetaOffline",
        "Proto_AlphaLoaded",
        "Proto_BetaLoaded",
        "Proto_ConfirmDeconstruction",
        "Proto_NoGateDetected",
        "Proto_MaxGatesPlaced",
        "Proto_NoDeploymentSpace",

        "Proto_OnTransmissionDeviceFirstLoaded",
        "Proto_OnCalibrationRunCompleted",
        "Proto_CalibrationPointReached",
        "Proto_CalibrationTestStarted",
        "Proto_CalibrationTestFailed",
        "Proto_StoryEndPingVoiceline",
        "Proto_AdminFacilityOnline",
        "Proto_DefensePingSpawned",
        "Proto_AdditionalData",
        "Proto_HullKeyUnlock",
        "Proto_ConstructionStart",
        "Proto_ConstructionFinish",
        "Proto_ApproachTerminal",
        "Proto_FirstInteract",
        "Proto_DeadZoneMappingImminent",
        "Proto_ReadyingDetectors",
        "Proto_PleaseDoNotProceed",
        "Proto_DeadZoneMappingInitialized"
    };
    
    public static void UpdateVoicelines()
    {
        string orionText1 = Language.main.Get("ProtoOrionNoData");
        string orionText2 = Language.main.Get("ProtoOrionFullData");
        string orionText3 = Language.main.Get("ProtoOrionUnknown");
        
        foreach (var key in VoicelineKeys)
        {
            string orionOld = Language.main.GetFormat(key, orionText1);
            string orionNew = Language.main.GetFormat(key, orionText2);
            string orionUnknown = Language.main.GetFormat(key, orionText3);
            LanguageHandler.SetLanguageLine(key + "_OrionUnknown", orionUnknown, Language.main.currentLanguage);
            LanguageHandler.SetLanguageLine(key + "_OrionNoData", orionOld, Language.main.currentLanguage);
            LanguageHandler.SetLanguageLine(key + "_OrionFullData", orionNew, Language.main.currentLanguage);
        }
    }
}