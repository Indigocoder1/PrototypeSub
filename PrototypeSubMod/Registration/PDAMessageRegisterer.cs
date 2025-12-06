using PrototypeSubMod.Patches;

namespace PrototypeSubMod.Registration;

internal static class PDAMessageRegisterer
{
    public static void Register()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        
        PDALog_Patches.entries.Add(("PDA_InterceptorUnlock", "OnInterceptorTestDataDownloaded"));
        PDALog_Patches.entries.Add(("PDA_OnDisableCloak", "OnDefenseCloakDisabled"));
        PDALog_Patches.entries.Add(("PDA_OnEnterMoonpool", "OnEnterDefenseMoonpool"));
        PDALog_Patches.entries.Add(("PDA_OnMoonpoolDisallow", "OnMoonpoolNoPrototype"));
        PDALog_Patches.entries.Add(("PDA_ApproachDefense", "OnApproachDefenseFacility"));
        PDALog_Patches.entries.Add(("PDA_NoFireExtinguisher", "NotifyPlayerNoExtinguishers"));
        PDALog_Patches.entries.Add(("PDA_Breach3Left", "PDA_Breach3Left"));
        PDALog_Patches.entries.Add(("PDA_Breach2Left", "PDA_Breach2Left"));
        PDALog_Patches.entries.Add(("PDA_Breach1Left", "PDA_Breach1Left"));
        PDALog_Patches.entries.Add(("PDA_Breach0Left", "PDA_Breach0Left"));
        PDALog_Patches.entries.Add(("PDA_OnEnterEngineFacility", "OnEnterEngineFacility"));
        PDALog_Patches.entries.Add(("PDA_OnTeleportToIsland", "OnInterceptorSequenceFinished"));
        PDALog_Patches.entries.Add(("PDA_OnEnterStoryEnd", "OnEnterStoryEndProximity"));
        PDALog_Patches.entries.Add(("PDA_OnEnterQEP", "OnEnterPrecursorGun"));
        PDALog_Patches.entries.Add(("PDA_OnEnterGrandReef", "ProtoOnEnterGrandReef"));
        PDALog_Patches.entries.Add(("PDA_ApproachEngineFacility", "ProtoApproachEngineFacility"));
        PDALog_Patches.entries.Add(("PDA_OnScanWyrm", "OnScanDisabledWyrm"));
        PDALog_Patches.entries.Add(("PDA_RevisitLater", "ProtoRevisitDefenseFacility"));
        PDALog_Patches.entries.Add(("PDA_RevisitLater", "ProtoRevisitInterceptorFacility"));
        PDALog_Patches.entries.Add(("PDA_FacilityLocations", "ProtoFacilityLocationsHint"));
        
        PDALog_Patches.entries.Add(("PlaceholderVoiceline", "ProtoBlinkFactorUnlock"));
        PDALog_Patches.entries.Add(("PlaceholderVoiceline", "ProtoBiomechanicsFactorUnlock"));
        PDALog_Patches.entries.Add(("PlaceholderVoiceline", "ProtoSuitColorFactorUnlock"));
        
        PDALog_Patches.orionEntries.Add(("Proto_StoryEndPingVoiceline", "Proto_StoryEndPingVoiceline"));
        PDALog_Patches.orionEntries.Add(("Proto_AdminFacilityOnline", "Proto_AdminFacilityOnline"));
        PDALog_Patches.orionEntries.Add(("Proto_ApproachTerminal", "Proto_ApproachTerminal"));
        PDALog_Patches.orionEntries.Add(("Proto_FirstInteract", "Proto_FirstInteract"));
        PDALog_Patches.orionEntries.Add(("Proto_ConstructionStart", "Proto_ConstructionStart"));
        PDALog_Patches.orionEntries.Add(("Proto_ConstructionFinish", "Proto_ConstructionFinish"));
        
        sw.Stop();
        Plugin.Logger.LogInfo($"PDA Messages registered in {sw.ElapsedMilliseconds}ms");
    }
}
