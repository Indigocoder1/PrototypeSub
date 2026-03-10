using System;
using System.Text;
using HarmonyLib;
using PriorityQueueInternal;
using PrototypeSubMod.Factors;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.Prefabs.AlienBuildingBlock;
using PrototypeSubMod.Prefabs.Factors;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(TooltipFactory))]
public class TooltipFactory_Patches
{
    public static event Action onRunItemActions;
    
    [HarmonyPatch(nameof(TooltipFactory.ItemCommons)), HarmonyPriority(Priority.High), HarmonyPostfix]
    private static void ItemCommons_Postfix(StringBuilder sb, TechType techType, GameObject obj)
    {
        if (techType == SuitColorFactor.prefabInfo.TechType)
        {
            HandleColorFactorTooltips(sb, obj);
        }

        if (techType == AlienBuildingBlock.prefabInfo.TechType)
        {
            HandleAlienBuildingBlockTooltips(sb, obj);
        }
    }

    private static void HandleColorFactorTooltips(StringBuilder sb, GameObject obj)
    {
        var colorFactor = obj.GetComponent<ColorFactor>();
        string localizedName = Language.main.Get(colorFactor.GetCurrentLocalizationKey());
        TooltipFactory.WriteDescription(sb, "────────────────");
        TooltipFactory.WriteDescription(sb, Language.main.GetFormat("SuitCurrentColor", localizedName));
        TooltipFactory.WriteDescription(sb, Language.main.GetFormat("SuitCurrentIntensity", colorFactor.GetIntensity()));
    }
    
    private static void HandleAlienBuildingBlockTooltips(StringBuilder sb, GameObject obj)
    {
        var eatable = obj.GetComponent<BiomechanicsEatable>();

        if (eatable.EatableActive())
        {
            TooltipFactory.WriteDescription(sb, Language.main.GetFormat("HealthFormat", eatable.GetHealthValue()));
        }
        
        var charge = eatable.GetIonCharge();
        var sign = Mathf.Sign(charge) < 0 ? "-" : "+";
        var text = Language.main.GetFormat("AlienBuildingBlockCharge", sign, charge.ToString("F0"));
        TooltipFactory.WriteDescription(sb, text);
    }

    [HarmonyPatch(nameof(TooltipFactory.ItemActions)), HarmonyPostfix]
    private static void ItemActions_Postfix(StringBuilder sb, InventoryItem item)
    {
        HandleColorFactorActions(sb, item);
        HandleLocatorFactorTooltips(sb, item);
        HandlePrecursorSuitTooltips(sb, item);

        onRunItemActions?.Invoke();
    }

    private static void HandleColorFactorActions(StringBuilder sb, InventoryItem item)
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
    
    private static void HandleLocatorFactorTooltips(StringBuilder sb, InventoryItem item)
    {
        if (item.techType != LocatorFactor.prefabInfo.TechType) return;

        var locatorFactor = item.item.GetComponent<Factors.Locator.Locator>();
        TooltipFactory.WriteAction(sb, GameInput.FormatButton(locatorFactor.GetUseButton()),
            Language.main.Get("LocatorToggle"));
    }

    private static void HandlePrecursorSuitTooltips(StringBuilder sb, InventoryItem item)
    {
        if (item.techType != PrecursorSuit.prefabInfo.TechType) return;

        TooltipFactory.WriteAction(sb, GameInput.FormatButton(GameInput.Button.AltTool), 
            Language.main.Get("PrecursorSuitRemnantToggle"));
        var key = Plugin.GlobalSaveData.precursorSuitGivesRemnants ? "LocalizedTrue" : "LocalizedFalse";
        var localizedRemnantValue = Language.main.Get(key);
        
        TooltipFactory.WriteDescription(sb,
            Language.main.GetFormat("PrecursorSuitRemnantState", localizedRemnantValue));
    }
}