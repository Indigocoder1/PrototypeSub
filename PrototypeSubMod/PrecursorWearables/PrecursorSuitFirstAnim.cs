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

        StoryGoalManager.main.OnGoalComplete("PrecursorSuitFirstInspect");
        UWE.CoroutineHost.StartCoroutine(DisableAnimParamDelayed());
    }

    private IEnumerator DisableAnimParamDelayed()
    {
        yield return null;
        
        Player.main.playerAnimator.SetBool("suit_first_use", false);
    }

    private void OnDestroy()
    {
        Inventory.main.equipment.onEquip -= OnEquip;
    }
}