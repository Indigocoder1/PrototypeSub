using PrototypeSubMod.DeployablesTerminal;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.UI.AbilitySelection;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class TransmissionDeviceLauncher : MonoBehaviour, IAbilityIcon
{
    [SerializeField] private DeployablesStorageTerminal deployableStorage;
    [SerializeField] private Transform launchOrigin;
    [SerializeField] private Sprite transmissionDeviceSprite;
    [SerializeField] private SelectionMenuManager selectionMenuManager;
    [SerializeField] private FMOD_CustomEmitter deploySFX;
    [SerializeField] private float launchDelay;
    [SerializeField] private float launchForce;

    private void Start()
    {
        deployableStorage.equipment.onEquip += OnItemChanged;
        deployableStorage.equipment.onUnequip += OnItemChanged;
    }

    private void OnItemChanged(string slot, InventoryItem inventoryItem)
    {
        if (inventoryItem.techType != ProtoTransmissionDevice.prefabInfo.TechType) return;

        selectionMenuManager.RefreshIcons();

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

        deviceItem.item.transform.position = launchOrigin.position;
        deviceItem.item.transform.forward = launchOrigin.forward;

        deviceItem.item.gameObject.SetActive(true);
        deviceItem.item.GetComponent<Rigidbody>().AddForce(launchOrigin.forward * launchForce, ForceMode.Impulse);
        deviceItem.item.GetComponent<TransmissionDeviceManager>().DeployDevice();
        deviceItem.item.transform.SetParent(null);
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

    public bool GetActive() => true;

    public bool GetCanActivate() => true;

    public bool GetShouldShow()
    {
        return HasTransmissionDevice();
    }

    public Sprite GetSprite()
    {
        return transmissionDeviceSprite;
    }

    public TechType GetTechType() => TechType.None;
}