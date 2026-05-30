using Nautilus.Handlers;
using PrototypeSubMod.DeployablesTerminal;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.UI.AbilitySelection;
using Story;
using System.Collections;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class TransmissionDeviceLauncher : MonoBehaviour, IAbilityIcon
{
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private DeployablesStorageTerminal deployableStorage;
    [SerializeField] private Transform launchOrigin;
    [SerializeField] private Sprite transmissionDeviceSprite;
    [SerializeField] private SelectionMenuManager selectionMenuManager;
    [SerializeField] private FMOD_CustomEmitter deploySFX;
    [SerializeField] private float launchDelay;
    [SerializeField] private float launchForce;

    private bool forceDisabled;
    
    private void Start()
    {
        deployableStorage.equipment.onEquip += OnItemChanged;
        deployableStorage.equipment.onUnequip += OnItemChanged;
    }

    private void OnItemChanged(string slot, InventoryItem inventoryItem)
    {
        if (inventoryItem.techType != ProtoTransmissionDevice.prefabInfo.TechType) return;

        selectionMenuManager.RefreshIcons();

        if (StoryGoalManager.main.IsGoalComplete("TransmissionDeviceFirstLoaded")) return;
        UWE.CoroutineHost.StartCoroutine(DelayedStoryGoalUnlock());
    }

    private IEnumerator DelayedStoryGoalUnlock()
    {
        yield return new WaitForSeconds(4f);
        StoryGoalManager.main.OnGoalComplete("TransmissionDeviceFirstLoaded");
    }

    private bool HasTransmissionDevice()
    {
        return deployableStorage.equipment.equippedCount.TryGetValue(ProtoTransmissionDevice.prefabInfo.TechType,
            out var count) && count > 0;
    }

    public void DeployDevice()
    {
        if (!HasTransmissionDevice())
        {
            ErrorMessage.AddError("No transmission device loaded!");
            return;
        }

        var deviceItem = deployableStorage.equipment.GetItemInSlot(DeployablesStorageTerminal.PHASE_GATE_SLOT);
        deployableStorage.equipment.RemoveItem(deviceItem.item);
        
        deviceItem.item.GetComponent<TransmissionDeviceManager>().DeployDevice(subRoot);
        deviceItem.item.transform.position = launchOrigin.position;
        deviceItem.item.transform.forward = launchOrigin.forward;

        var rb = deviceItem.item.GetComponent<Rigidbody>();
        UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, false);
        rb.AddForce(launchOrigin.forward * launchForce, ForceMode.Impulse);
    }

    private IEnumerator DeployDelayed()
    {
        deploySFX.Play();
        yield return new WaitForSeconds(launchDelay);

        DeployDevice();
    }

    public bool OnActivated()
    {
        UWE.CoroutineHost.StartCoroutine(DeployDelayed());
        return true;
    }

    public void OnSelectedChanged(bool changed) { }

    public bool GetActive() => false;

    public bool GetCanActivate() => !forceDisabled;

    public bool GetShouldShow()
    {
        return HasTransmissionDevice();
    }

    public void ForceDisabled()
    {
        forceDisabled = true;
    }

    public Sprite GetSprite()
    {
        return transmissionDeviceSprite;
    }

    public TechType GetTechType() => TechType.None;
    public bool GetIsInstalled() => true;
}