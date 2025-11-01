using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class TransmissionDeviceManager : MonoBehaviour, IItemSelectorManager
{
    [SerializeField] private Animator deviceAnimator;
    [SerializeField] private Animator cinematicAnimator;
    [SerializeField] private GameObject poweredDownObjects;
    [SerializeField] private GameObject poweredUpObjects;
    [SerializeField] private float activationDelay;

    private bool deployed;
    private bool activated;
    
    private void Start()
    {
        if (!Plugin.GlobalSaveData.activatedTransmissionDevices.Contains(GetComponent<PrefabIdentifier>().Id)) return;
        
        poweredDownObjects.SetActive(false);
        poweredUpObjects.SetActive(true);
        activated = true;
        deployed = true;
        deviceAnimator.SetTrigger("ActivateInstant");
        Destroy(GetComponent<Pickupable>());
    }

    public void OnHandHover(HandTargetEventData data)
    {
        if (!deployed)
        {
            HandReticle.main.SetText(HandReticle.TextType.Hand, "Deploy from prototype to use", true);
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny);
            return;
        }

        if (!activated)
        {
            HandReticle.main.SetText(HandReticle.TextType.Hand, "Insert power source", true,
                GameInput.Button.LeftHand);
            HandReticle.main.SetIcon(HandReticle.IconType.Hand);
            return;
        }
        
        HandReticle.main.SetText(HandReticle.TextType.Hand, "Enter transmission code", true,
            GameInput.Button.LeftHand);
        HandReticle.main.SetIcon(HandReticle.IconType.Hand);
    }

    public void OnHandClick(HandTargetEventData data)
    {
        if (!deployed) return;

        if (!activated)
        {
            uGUI.main.itemSelector.Initialize(this, SpriteManager.Get(SpriteManager.Group.Item, "nobattery"), 
                new List<IItemsContainer> {
                    Inventory.main.container
                });
        }
        else
        {
            OpenTransmissionCodePanel();
        }
    }

    private void OpenTransmissionCodePanel()
    {
        Player.main.pda.Open(Plugin.TransmissionEntryTab);
    }

    public bool Filter(InventoryItem item)
    {
        return item.techType is TechType.PrecursorIonCrystal or TechType.PrecursorIonCrystalMatrix;
    }

    public int Sort(List<InventoryItem> items)
    {
        // If there are no available items in the inventory
        if (items.Count == 0) return -1;
        
        items.Sort((a, b) =>
        {
            var nameA = Language.main.Get(a.techType);
            var nameB = Language.main.Get(b.techType);
            return String.Compare(nameB, nameA, StringComparison.Ordinal);
        });

        return 0;
    }

    public string GetText(InventoryItem item)
    {
        return Language.main.Get(item == null ? "ProtoCancelSelection" : item.item.GetTechName());
    }

    public void Select(InventoryItem item)
    {
        if (item == null) return;
        
        if (!Inventory.main.TryRemoveItem(item.item)) throw new Exception($"Could not remove {item.item} from inventory");

        Destroy(item.item.gameObject);

        StartCoroutine(ActivateDevice());
    }

    private IEnumerator ActivateDevice()
    {
        deviceAnimator.SetTrigger("Activate");
        Plugin.GlobalSaveData.activatedTransmissionDevices.Add(GetComponent<PrefabIdentifier>().Id);
        yield return new WaitForSeconds(activationDelay);
        
        ErrorMessage.AddDebug("Powered up transmission device");
        poweredDownObjects.SetActive(false);
        poweredUpObjects.SetActive(true);
        activated = true;
    }

    public void DeployDevice()
    {
        var pickupable = GetComponent<Pickupable>();
        if (pickupable)
        {
            Destroy(pickupable);
        }

        deployed = true;
    }

    public void PlayEndingCinematic()
    {
        cinematicAnimator.SetTrigger("PlayAnim");
        deviceAnimator.SetTrigger("Fire");
    }
}