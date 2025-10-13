using HarmonyLib;
using PrototypeSubMod.PrecursorWearables;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(EnergyInterface))]
public class EnergyInterface_Patches
{
    [HarmonyPatch(nameof(EnergyInterface.GetValues)), HarmonyPostfix]
    private static void GetValues_Postfix(EnergyInterface __instance, ref float charge, ref float capacity)
    {
        if (!__instance.TryGetComponent(out NoPropulsionEnergyTag _)) return;

        charge = 100;
        capacity = 100;
    }
    
    [HarmonyPatch(nameof(EnergyInterface.TotalCanProvide)), HarmonyPostfix]
    private static void TotalCanProvide_Postfix(EnergyInterface __instance, ref float __result)
    {
        if (!__instance.TryGetComponent(out NoPropulsionEnergyTag _)) return;

        __result = 100;
    }
    
    [HarmonyPatch(nameof(EnergyInterface.hasCharge)), HarmonyPostfix, HarmonyPatch(MethodType.Getter)]
    private static void HasCharge_Postfix(EnergyInterface __instance, ref bool __result)
    {
        if (!__instance.TryGetComponent(out NoPropulsionEnergyTag _)) return;

        __result = true;
    }
}