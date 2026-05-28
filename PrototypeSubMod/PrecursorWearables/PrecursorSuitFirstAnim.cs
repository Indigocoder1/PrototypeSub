using System.Collections;
using PrototypeSubMod.Prefabs;
using Story;
using UnityEngine;

namespace PrototypeSubMod.PrecursorWearables;

public class PrecursorSuitFirstAnim : MonoBehaviour
{
    private void Start()
    {
        Inventory.main.equipment.onEquip += OnEquip;
    }

    private void OnEquip(string slot, InventoryItem item) => CheckWearables();

    private void CheckWearables()
    {
        if (StoryGoalManager.main.IsGoalComplete("PrecursorSuitFirstInspect")) return;

        var itemInBody = Inventory.main.equipment.GetItemInSlot("Body");
        if (itemInBody == null || itemInBody.techType != PrecursorSuit.prefabInfo.TechType) return;

        var itemInGloves = Inventory.main.equipment.GetItemInSlot("Gloves");
        if (itemInGloves == null || itemInGloves.techType != PrecursorPropulsionGloves.PrefabInfo.TechType) return;

        Player.main.playerAnimator.SetBool("suit_first_use", true);
        
        PDALog.Add("PDA_Augments");

        var restoreQuickSlot = -1;
        if (Inventory.main.GetHeldTool() != null)
        {
            restoreQuickSlot = Inventory.main.quickSlots.activeSlot;
        }

        Inventory.main.quickSlots.SetSuspendSlotActivation(true);

        Inventory.main.ReturnHeld();
        StoryGoalManager.main.OnGoalComplete("PrecursorSuitFirstInspect");
        UWE.CoroutineHost.StartCoroutine(DisableAnimParamDelayed(restoreQuickSlot));
    }

    private IEnumerator DisableAnimParamDelayed(int restoreQuickSlot)
    {
        yield return new WaitForSeconds(4f);
        
        Player.main.playerAnimator.SetBool("suit_first_use", false);

        if (restoreQuickSlot != -1)
        {
            Inventory.main.quickSlots.Select(restoreQuickSlot);
        }
        
        Inventory.main.quickSlots.SetSuspendSlotActivation(false);
    }

    private void OnDestroy()
    {
        Inventory.main.equipment.onEquip -= OnEquip;
    }
}