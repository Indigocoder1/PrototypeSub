using HarmonyLib;
using PrototypeSubMod.Factors;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(uGUI_PDA))]
public class uGUI_PDA_Patches
{
    [HarmonyPatch(nameof(uGUI_PDA.Start)), HarmonyPostfix]
    private static void Start_Postfix(uGUI_PDA __instance)
    {
        __instance.GetComponentInChildren<uGUI_InventoryTab>(true).gameObject.EnsureComponent<FactorEquipmentManager>();
    }
    
    [HarmonyPatch(nameof(uGUI_PDA.OpenTab)), HarmonyPrefix]
    private static void OpenTab_Prefix(uGUI_PDA __instance)
    {
        __instance.gameObject.BroadcastMessage("RefreshFactorSlots", SendMessageOptions.RequireReceiver);
    }
}