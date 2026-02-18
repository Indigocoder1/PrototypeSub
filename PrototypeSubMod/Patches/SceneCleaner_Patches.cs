using HarmonyLib;
using PrototypeSubMod.PrototypeStory;
using PrototypeSubMod.Utility;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(SceneCleaner))]
public class SceneCleaner_Patches
{
    [SaveStateReference(false)]
    private static bool _queuedSceneOverride;
    
    [HarmonyPatch(nameof(SceneCleaner.Start)), HarmonyPrefix]
    private static void Start_Prefix(SceneCleaner __instance)
    {
        if (!_queuedSceneOverride) return;

        __instance.loadScene = "ProtoCredits";
        _queuedSceneOverride = false;
    }

    public static void QueueSceneOverride()
    {
        _queuedSceneOverride = true;
    }
}