using UnityEngine;

namespace PrototypeSubMod.Puzzles.MeasurePuzzle;

public class BeaconPlacementSlot : MonoBehaviour, IHandTarget
{
    [SerializeField] private PingInstance pingInstance;

    private void Start()
    {
        pingInstance.enabled = false;
    }

    public void OnHandHover(GUIHand hand)
    {
        HandReticle main = HandReticle.main;
        var useText = Language.main.GetFormat("MeasurePuzzleInputBeacon", GameInput.FormatButton(GameInput.Button.LeftHand));
        main.SetTextRaw(HandReticle.TextType.Hand, useText);
    }

    public void OnHandClick(GUIHand hand)
    {
        if (!Inventory.main.container._items.TryGetValue(TechType.Beacon, out var itemGroup) || itemGroup.items.Count == 0)
        {
            ErrorMessage.AddError(Language.main.Get("MeasurePuzzleNoBeacon"));
            return;
        }

        var item = itemGroup.items[0].item;
        if (!Inventory.main.TryRemoveItem(item))
        {
            ErrorMessage.AddError($"Failed to remove {item} from inventory!");
            throw new System.Exception($"Failed to remove {item} from inventory!");
        }

        Destroy(item.gameObject);
        pingInstance.enabled = true;
        Destroy(this);
    }
}