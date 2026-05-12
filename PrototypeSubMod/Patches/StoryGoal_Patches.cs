using HarmonyLib;
using PrototypeSubMod.Utility;
using Story;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(StoryGoal))]
public static class StoryGoal_Patches
{
    [SaveStateReference(false)]
    private static bool _blockGoalCompletion;
    
    [HarmonyPatch(nameof(StoryGoal.Execute)), HarmonyPrefix]
    private static bool Execute_Prefix()
    {
        return !_blockGoalCompletion;
    }

    public static void SetBlockGoalCompletion(bool blockCompletion)
    {
        _blockGoalCompletion = blockCompletion;
    }
}