using Nautilus.Handlers;
using PrototypeSubMod.Patches;

namespace PrototypeSubMod.Registration;

public static class RadioMessageRegisterer
{
    public static void Register()
    {
        #region Warper Radio Messages

        RegisterMessage("ProtoRadioMessage1", "ProtoRadioMessage1");
        RegisterMessage("ProtoRadioMessage2", "ProtoRadioMessage2");
        RegisterMessage("ProtoRadioMessage3", "ProtoRadioMessage3");
        RegisterMessage("ProtoRadioMessage4", "ProtoRadioMessage4");
        
        #endregion
    }

    private static void RegisterMessage(string key, string audioAssetName)
    {
        StoryGoalHandler.RegisterBiomeGoal(key, Story.GoalType.Radio, "Unobtanium", 1);
        PDALog_Patches.entries.Add((audioAssetName, key));
    }
}