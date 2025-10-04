using System;
using System.Collections;
using System.Linq;
using Nautilus.Json;
using PrototypeSubMod.Upgrades;
using UnityEngine;

namespace PrototypeSubMod.PhaseGates;

internal class ProtoPhaseGateManager : MonoBehaviour, IProtoEventListener
{
    public static event Action OnPhaseGateDeactivated;
    
    [SerializeField] private PrecursorTeleporter teleporter;
    [SerializeField] private PrefabIdentifier prefabIdentifier;
    
    private int gateIndex;
    private bool playerWasPiloting;
    private PhaseGateLocation connectedGateLocation;

    private void UpdateConnectedGate()
    {
        if (Plugin.GlobalSaveData.phaseGateLocations.Count % 2 == 1) return;
        
        int offset = -(gateIndex % 2 * 2 - 1);
        connectedGateLocation = Plugin.GlobalSaveData.phaseGateLocations.Values.ElementAt(gateIndex + offset);

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
    public void BeginTeleportSubRoot(SubRoot subRoot)
    {
        if (subRoot == null)
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

        StartCoroutine(TeleportSubRoot(subRoot.gameObject));
    }

    private IEnumerator TeleportSubRoot(GameObject subRoot)
    {
        PrecursorTeleporter.activeTeleporter = teleporter;
        var subRigidbody = subRoot.GetComponent<Rigidbody>();
        var pilotingChair = subRoot.GetComponentInChildren<PilotingChair>();

        playerWasPiloting = Player.main.currChair == pilotingChair;
        if (playerWasPiloting)
        {
            var player = Player.main;
            player.AddUsedTool(TechType.PrecursorTeleporter);

            player.cinematicModeActive = true;
            player.playerController.inputEnabled = false;
            Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);
            player.GetPDA().Close();
            player.GetPDA().SetIgnorePDAInput(true);
            player.teleportingLoopSound.Play();
            Player.mainCollider.enabled = false;
        
            player.onTeleportationComplete += () => OnTeleportationComplete(subRigidbody, pilotingChair);
            Camera.main.GetComponent<TeleportScreenFXController>().StartTeleport();
        }

        if (playerWasPiloting)
        {
            subRigidbody.isKinematic = true;
        }
        
        subRigidbody.velocity = Vector3.zero;

        yield return new WaitForSeconds(1f);
        
        subRoot.transform.position = teleporter.warpToPos;
        subRoot.transform.rotation = Quaternion.Euler(0, teleporter.warpToAngle, 0);

        if (playerWasPiloting)
        {
            Player.main.WaitForTeleportation();
        }
    }
    
    private void OnTeleportationComplete(Rigidbody subRigidbody, PilotingChair pilotingChair)
    {
        subRigidbody.isKinematic = false;
        Player.mainCollider.enabled = true;

        if (PrecursorTeleporter.activeTeleporter == teleporter)
        {
            PrecursorTeleporter.activeTeleporter = null;
        }

        StartCoroutine(ReEnterPilotingModeDelayed(pilotingChair));
        playerWasPiloting = false;
    }
    
    private IEnumerator ReEnterPilotingModeDelayed(PilotingChair pilotingChair)
    {
        yield return new WaitForEndOfFrame();
        
        Player.main.EnterPilotingMode(pilotingChair);
    }
    
    public void DeactivateGate()
    {
        TeleporterManager.main.activeTeleporters.Remove(teleporter.teleporterIdentifier);
        OnPhaseGateDeactivated?.Invoke();
    }

    private void OnGateDeactivated()
    {
        if (TeleporterManager.main.activeTeleporters.Contains(teleporter.teleporterIdentifier)) return;
        
        teleporter.ToggleDoor(false);
        teleporter.activeLoopSound.Stop();
    }
    
    private void OnEnable()
    {
        Plugin.GlobalSaveData.OnStartedSaving += OnStartedSaving;
        PhaseGateSubAbility.onPhaseGateCreated += UpdateConnectedGate;
        OnPhaseGateDeactivated += OnGateDeactivated;
    }

    private void OnDisable()
    {
        Plugin.GlobalSaveData.OnStartedSaving -= OnStartedSaving;
        PhaseGateSubAbility.onPhaseGateCreated -= UpdateConnectedGate;
        OnPhaseGateDeactivated -= OnGateDeactivated;
    }

    private void OnDestroy()
    {
        Plugin.GlobalSaveData.phaseGateIndices.Remove(prefabIdentifier.Id);
        Plugin.GlobalSaveData.phaseGateLocations.Remove(prefabIdentifier.Id);
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