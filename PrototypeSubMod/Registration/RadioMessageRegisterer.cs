using Nautilus.Handlers;
using PrototypeSubMod.Patches;
using Story;

namespace PrototypeSubMod.Registration;

public static class RadioMessageRegisterer
{
    public static void Register()
    {
        #region Transmissions

        RegisterMessage("ProtoRadioMessage1", "ProtoRadioMessage1");
        StoryGoalHandler.RegisterCompoundGoal("ProtoRadioMessage1", Story.GoalType.Radio, 300, "PlayerFirstPPTInteraction");
        RegisterMessage("ProtoRadioMessage2", "ProtoRadioMessage2");
        StoryGoalHandler.RegisterCompoundGoal("ProtoRadioMessage2", Story.GoalType.Radio, 1000, "ProtoRadioMessage1");
        RegisterMessage("ProtoRadioMessage3", "ProtoRadioMessage3");
        StoryGoalHandler.RegisterCompoundGoal("ProtoRadioMessage3", Story.GoalType.Radio, 300, "ProtoRadioMessage2", "HullFacilityWormTerminalEncy");
        
        RegisterMessage("ProtoRadioMessage4", "ProtoRadioMessage3");
        StoryGoalHandler.RegisterCompoundGoal("ProtoRadioMessage4", Story.GoalType.Radio, 120, "OnCalibrationRunCompleted");
        
        // Wyrm messages
        RegisterMessage("WyrmRadioMessageActivated", "WyrmRadioMessageActivated");
        StoryGoalHandler.RegisterCompoundGoal("WyrmRadioMessageActivated", Story.GoalType.Radio, 10, "HullFacilityWormTerminalEncy");
        
        RegisterMessage("WyrmRadioMessageVoid", "WyrmRadioMessageVoid");
        StoryGoalHandler.RegisterCompoundGoal("PDA_OnEnterVoidWyrmActivated", Story.GoalType.PDA, 0f, "WyrmRadioMessageVoid");
        #endregion
    }

    private static void RegisterMessage(string key, string audioAssetName)
    {
        StoryGoalHandler.RegisterBiomeGoal(key, Story.GoalType.Radio, "Unobtanium", 1);
        PDALog_Patches.entries.Add((audioAssetName, key));
    }
}