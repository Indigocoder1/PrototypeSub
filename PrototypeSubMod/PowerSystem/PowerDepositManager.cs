using System.Collections;
using System.Collections.Generic;
using PrototypeSubMod.Prefabs;
using UnityEngine;

namespace PrototypeSubMod.PowerSystem;

public class PowerDepositManager : MonoBehaviour, IItemSelectorManager
{
    private static readonly Dictionary<TechType, Vector3> PowerSourceScales = new()
    {
        { TechType.PrecursorIonCrystal, Vector3.one },
        { TechType.PrecursorIonCrystalMatrix, Vector3.one * 0.8f },
        { EngineFacilityKey.prefabInfo.TechType, Vector3.one * 0.9f },
        { IonPrism_Craftable.prefabInfo.TechType, Vector3.one * 5f },
    };
    
    private static readonly int HatchOpen = Animator.StringToHash("HatchOpen");
    private static readonly int AcceptSource = Animator.StringToHash("AcceptSource");
    private static readonly int PowerFull = Animator.StringToHash("PowerFull");
    
    [SerializeField] private PrototypePowerSystem powerSystem;
    [SerializeField] private PlayerCinematicController controller;
    [SerializeField] private Animator reactorAnimator;
    [SerializeField] private Animator cinematicAnimator;
    [SerializeField] private Transform itemHolder;
    [SerializeField] private FMOD_CustomEmitter approachSFX;
    [SerializeField] private FMOD_CustomEmitter depositSFX;
    [SerializeField] private float cinematicLength = 5f;
    
    private GameObject powerSourceObject;
    private int restoreQuickSlot = -1;
    private bool inAnimation;
    private bool reactorOpening;
    private bool inBounds;
    private bool reactorWasOpen;

    private void Start()
    {
        controller.animator = Player.main.playerAnimator;
        powerSystem.onReorderSources += UpdateReactorActive;
        powerSystem.onAllowedSourcesChanged += UpdateReactorActive;
    }

    public bool Filter(InventoryItem item)
    {
        return PrototypePowerSystem.AllowedPowerSources.ContainsKey(item.techType);
    }

    public int Sort(List<InventoryItem> items)
    {
        // If there are no available items in the inventory
        if (items.Count == 0) return -1;
        
        items.Sort((a, b) =>
        {
            var capacityA = a.item.GetComponent<PrototypePowerBattery>().capacity;
            var capacityB = b.item.GetComponent<PrototypePowerBattery>().capacity;
            return capacityB.CompareTo(capacityA);
        });
        
        return 0;
    }

    public string GetText(InventoryItem item)
    {
        if (item == null)
        {
            return Language.main.Get("ProtoCancelSelection");
        }
        
        return Language.main.Get(item.item.GetTechName());
    }

    public void Select(InventoryItem item)
    {
        if (item == null) return;

        controller.StartCinematicMode(Player.main);
        inAnimation = true;
        restoreQuickSlot = Inventory.main.quickSlots.activeSlot;
        Inventory.main.ReturnHeld();

        reactorAnimator.SetTrigger(AcceptSource);
        cinematicAnimator.SetTrigger(AcceptSource);
        StartCoroutine(ExitCinematicModeDelayed());

        if (!Inventory.main.TryRemoveItem(item.item)) throw new System.Exception($"Could not remove {item.item} from inventory");
        
        powerSourceObject = item.item.gameObject;
        powerSourceObject.transform.SetParent(itemHolder);
        powerSourceObject.transform.localPosition = Vector3.zero;
        if (PowerSourceScales.TryGetValue(item.techType, out var scale))
        {
            powerSourceObject.transform.localScale = scale;
        }
        
        powerSourceObject.transform.localRotation = Quaternion.identity;
        powerSourceObject.SetActive(true);
        var col = powerSourceObject.GetComponentInChildren<Collider>();
        if (col)
        {
            col.enabled = false;
        }

        if (powerSourceObject.TryGetComponent(out Rigidbody rb))
        {
            UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, true);
        }

        depositSFX.Play();
    }

    public void OpenSelection()
    {
        uGUI.main.itemSelector.Initialize(this, SpriteManager.Get(SpriteManager.Group.Item, "nobattery"), new List<IItemsContainer>
        {
            Inventory.main.container
        });
    }
    
    public void OnHover(HandTargetEventData eventData)
    {
        if (inAnimation) return;

        bool slotsFull = powerSystem.StorageSlotsFull();
        string text = slotsFull ? "ProtoPowerFull" : "UseProtoPowerSystem";
        var icon = slotsFull ? GameInput.Button.None : GameInput.Button.LeftHand;
        
        if (reactorOpening)
        {
            text = "ProtoReactorOpening";
            icon = GameInput.Button.None;
        }
        
        HandReticle main = HandReticle.main;
        main.SetText(HandReticle.TextType.Hand, text, true, icon);
        main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
        
        var handIcon = slotsFull || reactorOpening ? HandReticle.IconType.HandDeny : HandReticle.IconType.Hand;
        main.SetIcon(handIcon, 1f);
        
        reactorAnimator.SetBool(PowerFull, slotsFull);
    }

    public void OnUse(HandTargetEventData eventData)
    {
        if (inAnimation || reactorOpening) return;
        
        if (powerSystem.StorageSlotsFull()) return;

        OpenSelection();
    }
    
    public void OnPlayerCinematicModeEnd(PlayerCinematicController controller)
    {
        Inventory.main.quickSlots.Select(restoreQuickSlot);
    }

    public void OnPlayerProxyChanged(bool inBounds)
    {
        this.inBounds = inBounds;
        
        if (powerSystem.StorageSlotsFull()) return;

        UpdateReactorActive();
    }

    private void UpdateReactorActive()
    {
        reactorAnimator.SetBool(HatchOpen, inBounds);
        reactorAnimator.SetBool(PowerFull, false);

        if (inBounds && !reactorWasOpen)
        {
            approachSFX.Play();
        }

        reactorWasOpen = inBounds;
    }

    private IEnumerator ExitCinematicModeDelayed()
    {
        yield return new WaitForSeconds(cinematicLength);
        controller.EndCinematicMode();
    }

    public void SetInAnimation(bool inAnimation)
    {
        this.inAnimation = inAnimation;
    }

    public void SetOpeningDoors(bool opening)
    {
        reactorOpening = opening;
    }

    public void InstallCurrentPowerSource()
    {
        powerSourceObject.SetActive(false);
        string slot = "";
        for (int i = 0; i < PrototypePowerSystem.SLOT_NAMES.Length; i++)
        {
            var testSlot =  PrototypePowerSystem.SLOT_NAMES[i];
            if (powerSystem.equipment.GetItemInSlot(testSlot) == null)
            {
                slot = testSlot;
                break;
            }
        }
        
        var inventoryItem = powerSourceObject.GetComponent<Pickupable>().inventoryItem;
        powerSystem.equipment.AddItem(slot, inventoryItem);
        
        reactorAnimator.SetBool(PowerFull, powerSystem.StorageSlotsFull());
        reactorWasOpen = !powerSystem.StorageSlotsFull();

        UWE.CoroutineHost.StartCoroutine(NotifyNoFireExtinguishersDelayed());
    }

    private IEnumerator NotifyNoFireExtinguishersDelayed()
    {
        yield return new WaitForSeconds(12f);
        
        PDALog.Add("NotifyPlayerNoExtinguishers");
    }
}