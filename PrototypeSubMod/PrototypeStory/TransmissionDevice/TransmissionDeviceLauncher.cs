using PrototypeSubMod.DeployablesTerminal;
using PrototypeSubMod.Prefabs;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class TransmissionDeviceLauncher : MonoBehaviour
{
    [SerializeField] private DeployablesStorageTerminal deployableStorage;
    [SerializeField] private Transform launchOrigin;
    [SerializeField] private float launchForce;

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
}