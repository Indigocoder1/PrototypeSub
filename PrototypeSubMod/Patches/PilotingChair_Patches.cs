using HarmonyLib;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PilotingChair))]
public class PilotingChair_Patches
{
    [HarmonyPatch(nameof(PilotingChair.Update)), HarmonyPostfix]
    private static void Update_Postfix(PilotingChair __instance)
    {
        if (!__instance.TryGetComponent(out ProtoPilotingTooltip _)) return;
        
        if (__instance.currentPlayer != null && AvatarInputHandler.main.IsEnabled())
        {
            var exitString = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit);

            var strafeText = Language.main.Get("ProtoToggleStrafe");
            var formattedButton = GameInput.FormatButton(GameInput.Button.AltTool);
            var strafeString = Language.main.GetFormat("HandReticleAddButtonFormat", strafeText, formattedButton);
            HandReticle.main.SetTextRaw(HandReticle.TextType.Use, $"{exitString} | {strafeString}");
        }
    }
}