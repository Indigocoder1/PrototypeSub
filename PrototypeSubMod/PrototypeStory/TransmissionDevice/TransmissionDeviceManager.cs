using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrototypeSubMod.Credits;
using PrototypeSubMod.Patches;
using PrototypeSubMod.PrototypeStory.TransmissionCinematic;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class TransmissionDeviceManager : MonoBehaviour, IItemSelectorManager, IProtoTreeEventListener
{
    [SerializeField] private DeviceCinematicManager cinematicManager;
    [SerializeField] private Animator deviceAnimator;
    [SerializeField] private GameObject poweredDownObjects;
    [SerializeField] private GameObject poweredUpObjects;
    [SerializeField] private float activationDelay;
    
    [Header("SFX")]
    [SerializeField] private FMOD_CustomEmitter activateSfx;
    [SerializeField] private FMOD_CustomEmitter idleSfx;

    private SubRoot ownerSub;
    private bool deployed;
    private bool activated;
    private bool pdaOpen;
    
    private void Start()
    {
        uGUI_PDA.main.GetComponentInChildren<uGUI_TransmissionTab>(true).onTransmissionComplete += PlayEndingCinematic;
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
        var deviceID = GetComponent<PrefabIdentifier>().Id;
        var ownerID = ownerSub.GetComponent<PrefabIdentifier>().Id;
        Plugin.GlobalSaveData.activatedTransmissionDevices.Add(deviceID, ownerID);
        activateSfx.Play();
        yield return new WaitForSeconds(activationDelay);
        
        poweredDownObjects.SetActive(false);
        poweredUpObjects.SetActive(true);
        activated = true;
        idleSfx.Play();
    }

    public void DeployDevice(SubRoot subDeployedFrom)
    {
        deployed = true;
        ownerSub = subDeployedFrom;
        GetComponent<Pickupable>().Drop();
    }

    // Called via SendMessage in Pickupable
    private void OnExamine()
    {
        deployed = false;
        activated = false;
        poweredDownObjects.SetActive(true);
        poweredUpObjects.SetActive(false);
        idleSfx.Stop();
        deviceAnimator.SetTrigger("Deactivate");
        Plugin.GlobalSaveData.activatedTransmissionDevices.Remove(GetComponent<PrefabIdentifier>().Id);
    }

    public void PlayEndingCinematic()
    {
        Player.main.pda.Close();
        Player.main.pda.Deactivated();
        Player.main.SetScubaMaskActive(false);
        HideForScreenshots.Hide(HideForScreenshots.HideType.HUD);
        Player_Patches.SetOxygenReqOverride(true, 0);
        IngameMenu_Patches.SetDenySaving(true);
        Player.main.cinematicModeActive = true;
        Player.main.playerController.SetEnabled(false);
        Inventory.main.quickSlots.DeselectImmediate();
        Player.main.FreezeStats();
        BreathingSound_Patches.SetStopBreathingSounds(true);

        var transmissionCinematic = ownerSub.GetComponentInChildren<SubTransmissionCinematic>();
        transmissionCinematic.PlayCinematic(cinematicManager);

        HideForScreenshots.Hide(HideForScreenshots.HideType.Mask | HideForScreenshots.HideType.HUD | HideForScreenshots.HideType.ViewModel);
        GUIController_Patches.SetDenyHideCycling(true);
    }

    public void FadeToBlack()
    {
        ProtoScreenFadeManager.instance.FadeIn(1);
    }

    public void OnCinematicFinished()
    {
        ErrorMessage.AddError("Cinematic finished");
        Player_Patches.SetOxygenReqOverride(false, 0);
        HideForScreenshots.Hide(HideForScreenshots.HideType.None);
        IngameMenu_Patches.SetDenySaving(false);
        Player.main.SetHeadVisible(false);
        Player.main.playerController.SetEnabled(true);
        Player.main.cinematicModeActive = false;
        Player.main.UnfreezeStats();
        GUIController_Patches.SetDenyHideCycling(false);
        BreathingSound_Patches.SetStopBreathingSounds(false);
        PlayCredits();
    }
    
    private void PlayCredits()
    {
        ProtoCreditsManager.QueueTransmissionEnding = true;
        FMODUnity.RuntimeManager.StopAllEvents(true);
        SceneCleaner_Patches.QueueSceneOverride();
        SceneCleaner.Open();
    }

    private void OnEnable()
    {
        if (!Plugin.GlobalSaveData.activatedTransmissionDevices.ContainsKey(GetComponent<PrefabIdentifier>().Id)) return;
        
        poweredDownObjects.SetActive(false);
        poweredUpObjects.SetActive(true);
        activated = true;
        deployed = true;
        deviceAnimator.SetTrigger("ActivateInstant");
        idleSfx.Play();
    }

    private void OnDestroy()
    {
        OnCinematicFinished();
    }

    public void OnProtoSerializeObjectTree(ProtobufSerializer serializer) { }
    
    public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
    {
        var deviceID = GetComponent<PrefabIdentifier>().Id;
        if (!Plugin.GlobalSaveData.activatedTransmissionDevices.TryGetValue(deviceID, out var subID)) return;

        var subRoots = Resources.FindObjectsOfTypeAll(typeof(SubRoot)).Select(s => (SubRoot)s);
        var ownerSub = subRoots.FirstOrDefault(i => i.GetComponent<PrefabIdentifier>().Id == subID);
        if (ownerSub == null)
        {
            Plugin.Logger.LogWarning($"Didn't find a SubRoot with id = {subID} in the scene. Resetting {deviceID} device.");
            OnExamine();
            return;
        }

        this.ownerSub = ownerSub;
    }
}