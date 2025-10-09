using HarmonyLib;
using PrototypeSubMod.Factors;
using TMPro;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(uGUI_EquipmentSlot))]
public class uGUI_EquipmentSlot_Patches
{
    /*
    [HarmonyPatch(nameof(uGUI_EquipmentSlot.)), HarmonyPrefix]
    private static bool OnButtonDown_Prefix(uGUI_EquipmentSlot __instance)
    {
        Plugin.Logger.LogInfo($"Button down on {__instance.slot}");
        if (__instance.slot != "Body") return true;

        bool hasFactor = false;
        foreach (var slot in FactorEquipmentManager.FactorSlots)
        {
            if (Inventory.main.equipment.GetItemInSlot(slot) != null)
            {
                hasFactor = true;
                break;
            }
        }

        if (!hasFactor) return true;

        ErrorMessage.AddError(Language.main.Get("ProtoSuitUnequipWarning"));
        return false;
    }
    */
}