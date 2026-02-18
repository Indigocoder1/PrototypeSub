using System;
using System.Collections;
using System.Collections.Generic;
using PrototypeSubMod.Patches;
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
    private bool pdaOpen;
    
    private void Start()
    {
        uGUI_PDA.main.GetComponentInChildren<uGUI_TransmissionTab>(true).onTransmissionComplete += PlayEndingCinematic;
        
        if (!Plugin.GlobalSaveData.activatedTransmissionDevices.Contains(GetComponent<PrefabIdentifier>().Id)) return;
        
        poweredDownObjects.SetActive(false);
        poweredUpObjects.SetActive(true);
        activated = true;
        deployed = true;
        deviceAnimator.SetTrigger("ActivateInstant");
        Destroy(GetComponent<Pickupable>());
    }

    private void Update()
    {
        if (!pdaOpen) return;

        const float maxPdaDistance = 7;
        if ((transform.position - Player.main.transform.position).sqrMagnitude > maxPdaDistance * maxPdaDistance)
        {
            Player.main.pda.Close();
        }
    }

    public void OnHandHover(HandTargetEventData data)
    {
        if (!deployed)
        {
            HandReticle.main.SetText(HandReticle.TextType.Hand, "ProtoTransmissionDeviceDeploy", true);
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny);
            return;
        }

        if (!activated)
        {
            HandReticle.main.SetText(HandReticle.TextType.Hand, "ProtoTransmissionDevicePower", true,
                GameInput.Button.LeftHand);
            HandReticle.main.SetIcon(HandReticle.IconType.Hand);
            return;
        }
        
        HandReticle.main.SetText(HandReticle.TextType.Hand, "ProtoTransmissionDeviceCode", true,
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
        Player.main.pda.Open(Plugin.TransmissionEntryTab, onCloseCallback: _ => pdaOpen = false);
        pdaOpen = true;
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
        Player.main.pda.Close();
        HideForScreenshots.Hide(HideForScreenshots.HideType.HUD);
        Player_Patches.SetOxygenReqOverride(true, 0);
        IngameMenu_Patches.SetDenySaving(true);
        Player.main.SetHeadVisible(true);
        Player.main.playerController.SetEnabled(false);
        cinematicAnimator.SetTrigger("PlayAnim");
        deviceAnimator.SetTrigger("Fire");
    }

    public void OnCinematicFinished()
    {
        ErrorMessage.AddError("Cinematic finished");
        Player_Patches.SetOxygenReqOverride(false, 0);
        HideForScreenshots.Hide(HideForScreenshots.HideType.None);
        IngameMenu_Patches.SetDenySaving(false);
        Player.main.SetHeadVisible(false);
        Player.main.playerController.SetEnabled(true);
        PlayCredits();
    }
    
    private void PlayCredits()
    {
        FMODUnity.RuntimeManager.StopAllEvents(true);
        SceneCleaner_Patches.QueueSceneOverride();
        SceneCleaner.Open();
    }
}