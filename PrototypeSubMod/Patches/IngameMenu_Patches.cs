using HarmonyLib;
using PrototypeSubMod.Utility;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(IngameMenu))]
internal class IngameMenu_Patches
{
    [SaveStateReference(false)]
    private static bool _denySaving = false;

    [HarmonyPatch(nameof(IngameMenu.GetAllowSaving)), HarmonyPostfix]
    private static void GetAllowSaving_Postfix(ref bool __result)
    {
        if (!_denySaving) return;

        __result = false;
    }

    public static void SetDenySaving(bool denySaving)
    {
        _denySaving = denySaving;
    }
}
