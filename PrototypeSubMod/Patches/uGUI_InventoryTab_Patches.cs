using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PrototypeSubMod.VehicleAccess;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(uGUI_InventoryTab))]
internal class uGUI_InventoryTab_Patches
{
    [HarmonyPatch(nameof(uGUI_InventoryTab.OnPointerEnter)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> OnPointerEnter_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatch match = new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "GetEquipmentType");

        FieldInfo containerInfo = typeof(InventoryItem).GetField("container");

        var matcher = new CodeMatcher(instructions)
            .MatchForward(true, match)
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, containerInfo))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .Insert(Transpilers.EmitDelegate(Inventory_Patches.GetModifiedEquipmentTypeItemsContainer));

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(nameof(uGUI_InventoryTab.Awake)), HarmonyPostfix]
    private static void Awake_Postfix(uGUI_InventoryTab __instance)
    {
        GameObject storageAccess = GameObject.Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("VehicleStorageBackButton"), __instance.transform);
        storageAccess.transform.localPosition = new Vector3(20, 260, 0);
        storageAccess.SetActive(false);
    }

    [HarmonyPatch(nameof(uGUI_InventoryTab.GetNavigableGridInDirection)), HarmonyPostfix]
    private static void GetNavigableGridInDirection_Postfix(uGUI_InventoryTab __instance,
        ref uGUI_INavigableIconGrid __result, uGUI_INavigableIconGrid grid, int dirX, int dirY)
    {
        var returnManager = __instance.usedStorageGrids.FirstOrDefault(i => i is VehicleAccessReturnManager);
        if (returnManager == null) return;

        if (__result == returnManager) return;

        Plugin.Logger.LogInfo($"Grid = {grid} | Result = {__result}");
        
        if (__result == null)
        {
            __result = __instance.usedStorageGrids[0];
            Plugin.Logger.LogInfo($"Setting result to {__instance.usedStorageGrids[0]}");
            return;
        }
        
        var startContainer = grid as uGUI_ItemsContainer;
        if (startContainer == null && grid == __instance.usedStorageGrids[0])
        {
            __result = returnManager;
            return;
        }
        
        var resultContainer = __result as uGUI_ItemsContainer;
        if (!resultContainer)
        {
            if (grid == (uGUI_INavigableIconGrid)__instance.inventory)
            {
                __result = returnManager;
                Plugin.Logger.LogInfo($"Setting result to {returnManager}");
                return;
            }
            
            if (grid != returnManager)
            {
                __result = returnManager;
                Plugin.Logger.LogInfo($"Setting result to {returnManager}");
                return;
            }
        }
        
        if (startContainer && resultContainer == __instance.inventory)
        {
            __result = returnManager;
            return;
        }

        if (grid == returnManager && __result == __instance.inventory)
        {
            return;
        }
        
        var localMove = __instance.transform.TransformDirection(new Vector3(dirX, -(float)dirY, 0f));
        var rotation = Quaternion.Inverse(__instance.transform.rotation) * localMove;
        var startPosition = startContainer.transform.TransformPoint(uGUI_InventoryTab.GetPointOnRectEdge(startContainer.transform as RectTransform, rotation));
        float oldMoveScore = __instance.GetMoveBetweenScore(startContainer, startPosition, resultContainer, localMove);
        if (GetMoveScore(grid, returnManager as VehicleAccessReturnManager, startPosition, new Vector3(-localMove.y, localMove.x)) > oldMoveScore)
        {
            __result = returnManager;
        }
    }

    private static float GetMoveScore(uGUI_INavigableIconGrid gridStart, VehicleAccessReturnManager returnManager, Vector3 currentPos, Vector3 dir)
    {
        if (gridStart == (uGUI_INavigableIconGrid)returnManager) return 0;

        if (!returnManager.gameObject.activeSelf) return 0;

        Vector3 center = returnManager.GetRect().rect.center;
        Vector3 delta = returnManager.transform.TransformPoint(center) - currentPos;
        float dot = Vector3.Dot(dir, delta);
        if (dot <= 0) return float.NegativeInfinity;

        return dot / delta.sqrMagnitude;
    }

    [HarmonyPatch(nameof(uGUI_InventoryTab.OnPointerClick)), HarmonyPostfix]
    private static void OnPointerClick_Postfix(uGUI_InventoryTab __instance)
    {
        __instance.SendMessage("RefreshFactorSlots");
    }
}
