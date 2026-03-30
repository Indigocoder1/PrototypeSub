using System;
using HarmonyLib;
using PrototypeSubMod.Utility;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(IngameMenu))]
internal class IngameMenu_Patches
{
    [SaveStateReference(false)]
    private static bool _denySaving;

    public static event Action OnQuitToMainMenu;

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

    [HarmonyPatch(nameof(IngameMenu.QuitToMainMenuAsync)), HarmonyPrefix, HarmonyPatch(MethodType.Enumerator)]
    private static void QuitToMainMenuAsync_Prefix()
    {
        OnQuitToMainMenu?.Invoke();
    }
}
