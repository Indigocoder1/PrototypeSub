using System;
using System.Linq;
using Nautilus.Json;
using UnityEngine;

namespace PrototypeSubMod.PhaseGates;

public class ProtoPhaseGateManager : MonoBehaviour, IProtoEventListener
{
    [SerializeField] private PrecursorTeleporter teleporter;
    [SerializeField] private PrefabIdentifier prefabIdentifier;
    
    private int gateIndex;
    private PhaseGateLocation connectedGateLocation;

    private void UpdateConnectedGate()
    {
        if (Plugin.GlobalSaveData.phaseGateLocations.Count % 2 == 1) return;
        
        int offset = -(gateIndex % 2 * 2 - 1);
        connectedGateLocation = Plugin.GlobalSaveData.phaseGateLocations[gateIndex + offset];

        teleporter.warpToPos = connectedGateLocation.Position + connectedGateLocation.TeleporterForward * 50;
        var forward = connectedGateLocation.TeleporterForward;
        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        teleporter.warpToAngle = angle;
    }

    public void SetGateIndex(int index)
    {
        gateIndex = index;
        Plugin.GlobalSaveData.phaseGateIndices[prefabIdentifier.Id] = gateIndex;
        UpdateConnectedGate();
    }

    public void ActivateGate()
    {
        teleporter.ToggleDoor(true);
    }
    
    public void DeactivateGate()
    {
        teleporter.ToggleDoor(false);
        teleporter.activeLoopSound.Stop();
        TeleporterManager.main.activeTeleporters.Remove(teleporter.teleporterIdentifier);
    }
    
    private void OnEnable()
    {
        Plugin.GlobalSaveData.OnStartedSaving += OnStartedSaving;
        PhaseGateSubAbility.onPhaseGateCreated += UpdateConnectedGate;
    }

    private void OnDisable()
    {
        Plugin.GlobalSaveData.OnStartedSaving -= OnStartedSaving;
        PhaseGateSubAbility.onPhaseGateCreated -= UpdateConnectedGate;
    }

    private void OnStartedSaving(object sender, JsonFileEventArgs args)
    {
        Plugin.GlobalSaveData.phaseGateIndices[prefabIdentifier.Id] = gateIndex;
    }

    public void OnProtoSerialize(ProtobufSerializer serializer) { }

    public void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        if (!Plugin.GlobalSaveData.phaseGateIndices.TryGetValue(prefabIdentifier.Id, out gateIndex)) return;
        
        UpdateConnectedGate();
    }
}