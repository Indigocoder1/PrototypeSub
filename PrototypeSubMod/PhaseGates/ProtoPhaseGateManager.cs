using System;
using System.Collections;
using System.Linq;
using Nautilus.Json;
using PrototypeSubMod.Upgrades;
using UnityEngine;

namespace PrototypeSubMod.PhaseGates;

internal class ProtoPhaseGateManager : MonoBehaviour, IProtoEventListener
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

    // Called via SendMessageUpwards in PrecursorTeleporterCollider
    public void BeginTeleportPrototype(ProtoUpgradeManager upgradeManager)
    {
        if (upgradeManager == null)
        {
            return;
        }
        if (PrecursorTeleporter.activeTeleporter != null)
        {
            return;
        }
        if (!TeleporterManager.GetTeleporterActive(teleporter.teleporterIdentifier))
        {
            return;
        }
        
        Plugin.Logger.LogInfo($"Upgrade manager = {upgradeManager}");
        if (!upgradeManager) return;

        StartCoroutine(TeleportPrototype(upgradeManager.gameObject));
    }

    private IEnumerator TeleportPrototype(GameObject prototypeObj)
    {
        PrecursorTeleporter.activeTeleporter = teleporter;
        
        var player = Player.main;
        player.AddUsedTool(TechType.PrecursorTeleporter);

        player.cinematicModeActive = true;
        player.playerController.inputEnabled = false;
        Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);
        player.GetPDA().Close();
        player.GetPDA().SetIgnorePDAInput(true);
        player.teleportingLoopSound.Play();
        player.GetComponent<Collider>().enabled = false;
        var subRigidbody = prototypeObj.GetComponent<Rigidbody>();
        var pilotingChair = prototypeObj.GetComponentInChildren<PilotingChair>();
        
        player.onTeleportationComplete += () => OnTeleportationComplete(subRigidbody, pilotingChair);
        
        Camera.main.GetComponent<TeleportScreenFXController>().StartTeleport();
        subRigidbody.isKinematic = true;
        subRigidbody.velocity = Vector3.zero;

        Player.main.mode = Player.Mode.LockedPiloting;

        yield return new WaitForSeconds(1f);

        var rotation = Quaternion.Euler(0, teleporter.warpToAngle, 0);

        prototypeObj.transform.position = teleporter.warpToPos;
        prototypeObj.transform.rotation = rotation;

        Player.main.WaitForTeleportation();
    }
    
    private void OnTeleportationComplete(Rigidbody subRigidbody, PilotingChair pilotingChair)
    {
        subRigidbody.isKinematic = false;
        Player.main.GetComponent<Collider>().enabled = true;

        if (PrecursorTeleporter.activeTeleporter == teleporter)
        {
            PrecursorTeleporter.activeTeleporter = null;
        }

        StartCoroutine(ReEnterPilotingModeDelayed(pilotingChair));
    }
    
    private IEnumerator ReEnterPilotingModeDelayed(PilotingChair pilotingChair)
    {
        yield return new WaitForEndOfFrame();
        
        Player.main.EnterPilotingMode(pilotingChair);
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