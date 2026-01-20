using Nautilus.Handlers;
using PrototypeSubMod.Patches;

namespace PrototypeSubMod.Registration;

public static class RadioMessageRegisterer
{
    public static void Register()
    {
        #region Warper Radio Messages

        RegisterMessage("ProtoRadioMessage1", "ProtoRadioMessage1");
        StoryGoalHandler.RegisterCompoundGoal("ProtoRadioMessage1", Story.GoalType.Radio, 300, "PrototypeCrafted");
        RegisterMessage("ProtoRadioMessage2", "ProtoRadioMessage2");
        StoryGoalHandler.RegisterCompoundGoal("ProtoRadioMessage2", Story.GoalType.Radio, 3600, "ProtoRadioMessage1");
        RegisterMessage("ProtoRadioMessage3", "ProtoRadioMessage3");
        StoryGoalHandler.RegisterCompoundGoal("ProtoRadioMessage3", Story.GoalType.Radio, 3600, "ProtoRadioMessage2");
        
        RegisterMessage("ProtoRadioMessage4", "ProtoRadioMessage3");
        StoryGoalHandler.RegisterCompoundGoal("ProtoRadioMessage4", Story.GoalType.Radio, 120, "OnCalibrationRunCompleted");
        #endregion
    }

    private static void RegisterMessage(string key, string audioAssetName)
    {
        StoryGoalHandler.RegisterBiomeGoal(key, Story.GoalType.Radio, "Unobtanium", 1);
        PDALog_Patches.entries.Add((audioAssetName, key));
    }
}