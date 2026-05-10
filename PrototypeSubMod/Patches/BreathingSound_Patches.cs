using HarmonyLib;
using PrototypeSubMod.Utility;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(BreathingSound))]
public static class BreathingSound_Patches
{
    [SaveStateReference(false)]
    private static bool StopBreathingSounds;
    
    [HarmonyPatch(nameof(BreathingSound.UpdateSound)), HarmonyPrefix]
    private static bool UpdateSound_Prefix()
    {
        return !StopBreathingSounds;
    }

    public static void SetStopBreathingSounds(bool stopBreathingSounds)
    {
        StopBreathingSounds = stopBreathingSounds;
    }
}