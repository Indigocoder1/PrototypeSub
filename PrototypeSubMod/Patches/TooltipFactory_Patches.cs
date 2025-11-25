using System.Text;
using HarmonyLib;
using PrototypeSubMod.Factors;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.Prefabs.Factors;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(TooltipFactory))]
public class TooltipFactory_Patches
{
    [HarmonyPatch(nameof(TooltipFactory.ItemCommons)), HarmonyPriority(Priority.High), HarmonyPostfix]
    private static void ItemCommons_Postfix(StringBuilder sb, TechType techType, GameObject obj)
    {
        if (techType == SuitColorFactor.prefabInfo.TechType)
        {
            HandleColorFactorTooltips(sb, obj);
        }

        if (techType == PrecursorSuit.PrefabInfo.TechType)
        {
            HandlePrecursorSuitTooltips(sb, obj);
        }
    }

    private static void HandleColorFactorTooltips(StringBuilder sb, GameObject obj)
    {
        var colorFactor = obj.GetComponent<ColorFactor>();
        string localizedName = Language.main.Get(colorFactor.GetCurrentLocalizationKey());
        TooltipFactory.WriteDescription(sb, "────────────────");
        TooltipFactory.WriteDescription(sb, Language.main.GetFormat("SuitCurrentColor", localizedName));
        TooltipFactory.WriteDescription(sb, Language.main.GetFormat("SuitCurrentIntensity", colorFactor.GetIntensity()));
        
        colorFactor.UpdateFromUI();
    }

    private static void HandlePrecursorSuitTooltips(StringBuilder sb, GameObject obj)
    {
        var ionManager = obj.GetComponent<FactorIonManager>();
        var text = Language.main.GetFormat("FactorIonCharge", (ionManager.GetNormalizedCharge() * 100).ToString("F0"));
        TooltipFactory.WriteDescription(sb, text);
    }

    [HarmonyPatch(nameof(TooltipFactory.ItemActions)), HarmonyPostfix]
    private static void ItemActions_Postfix(StringBuilder sb, InventoryItem item)
    {
        if (item.techType != SuitColorFactor.prefabInfo.TechType) return;
        
        var colorFactor = item.item.GetComponent<ColorFactor>();
        string editKey = colorFactor.GetIsEditingColor() ? "Color" : "Intensity";
        
        TooltipFactory.WriteAction(sb, GameInput.FormatButton(colorFactor.GetNextButton()),
            Language.main.Get($"Suit{editKey}Next"));
        TooltipFactory.WriteAction(sb, GameInput.FormatButton(colorFactor.GetPrevButton()),
            Language.main.Get($"Suit{editKey}Prev"));
        TooltipFactory.WriteAction(sb, GameInput.FormatButton(GameInput.Button.AltTool),
            Language.main.Get("SuitToggleEditMode"));
    }
}