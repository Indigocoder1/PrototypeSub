using System;
using System.Collections;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using UnityEngine;

namespace PrototypeSubMod.Docking;

public class ProtoDockingManager : MonoBehaviour, IProtoEventListener, IProtoTreeEventListener
{
    public event Action onDockedStatusChanged;
    
    [SerializeField] private CanvasGroup hudCanvasGroup;
    [SerializeField] private InterfloorTeleporter interfloorTeleporter;
    [SerializeField] private VehicleDockingBay dockingBay;
    [SerializeField] private IgnoreCinematicStart ignoreCinematicStart;
    [SerializeField] private FinsDockingManager finsDockingManager;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Transform vehicleHolder;

    private Vehicle playersVehicle;
    private Vehicle lastDockedVehicle;
    private bool cinematicStartManaged;
    
    private void Start()
    {
        if (cinematicStartManaged) return;
        
        ignoreCinematicStart.enabled = false;
    }

    private IEnumerator InitializeVehicle()
    {
        if (vehicleHolder.childCount == 0) yield break;

        cinematicStartManaged = true;
        ignoreCinematicStart.enabled = true;

        yield return new WaitForEndOfFrame();
        
        var vehicle = vehicleHolder.GetChild(0).gameObject;
        vehicle.SetActive(true);
        
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => dockingBay._dockedVehicle);
        
        StoreVehicle();
        ignoreCinematicStart.enabled = false;
        cinematicStartManaged = false;
    }
    
    public void TeleportIntoSub()
    {
        if (dockingBay.dockedVehicle == playersVehicle)
        {
            interfloorTeleporter.StartTeleportPlayer();
        }

        Invoke(nameof(StoreVehicle), 0.1f);
    }

    public void SetPlayersVehicle(Vehicle vehicle)
    {
        playersVehicle = vehicle;
    }

    public void StoreVehicle()
    {
        if (!dockingBay.dockedVehicle) return;
        
        dockingBay.dockedVehicle.transform.SetParent(vehicleHolder);
        dockingBay.dockedVehicle.gameObject.SetActive(false);
        dockingBay.subRoot.voiceNotificationManager.TryPlayNext();
        
        onDockedStatusChanged?.Invoke();
        finsDockingManager.SetDockingPrep(false);
        finsDockingManager.GetComponent<ProtoFinsManager>().ResetFinAnimations();

        dockingBay.dockedVehicle.GetComponent<WorldForces>().waterDepth = Ocean.GetOceanLevel();

        VehicleFrameworkCompatManager.TryEndDocking(dockingBay.dockedVehicle);
    }

    public void Undock()
    {
        UWE.CoroutineHost.StartCoroutine(UndockDelayed());
    }

    public bool DockUnlocked() => finsDockingManager.GetFinsManager().GetInstalledFinCount() >= ProtoFinsManager.FinsForDocking;

    // Called via VehicleDockingBay Invoke
    private void UnlockDoors()
    {
        if (!dockingBay.dockedVehicle || dockingBay.dockedVehicle == lastDockedVehicle) return;

        lastDockedVehicle = dockingBay.dockedVehicle;
        
        var localPos = dockingBay.dockedVehicle.transform.InverseTransformPoint(dockingBay.dockedVehicle.playerPosition.transform.position);
        playerPosition.localPosition = localPos;
    }
    
    private IEnumerator UndockDelayed()
    {
        if (!dockingBay.dockedVehicle) yield break;
        
        FMODUWE.PlayOneShot(interfloorTeleporter.GetFMODAsset(), transform.position, 0.25f);
        InterfloorTeleporter.PlayTeleportEffect(0.2f);
        
        yield return new WaitForSeconds(0.1f);

        var chair = Player.main.currChair;
        if (chair)
        {
            UndockSkipCinematic();
        }

        hudCanvasGroup.alpha = 0;
        var vehicle = vehicleHolder.GetChild(0).gameObject;
        dockingBay.OnUndockingComplete(Player.main);
        vehicle.SetActive(true);
        var vehicleComp = vehicle.GetComponent<Vehicle>();
        vehicleComp.UpdateCollidersForDocking(true);
        
        onDockedStatusChanged?.Invoke();
        
        yield return new WaitForSeconds(0.2f);
        vehicleComp.UpdateCollidersForDocking(false);
        VehicleFrameworkCompatManager.OnVehicleUndocked(vehicleComp);
        
        if (vehicleComp.controlSheme == Vehicle.ControlSheme.Mech) yield break;
        
        if (!vehicleComp.useRigidbody) yield break;
        
        if (chair)
        {
            chair.releaseCinematicController.animator.SetBool(chair.releaseCinematicController.animParam, false);
        }

        var rb = vehicleComp.useRigidbody;
        rb.AddForce((rb.transform.forward - rb.transform.up) * 10f, ForceMode.VelocityChange);
    }

    private void UndockSkipCinematic()
    {
        GameInput.ClearInput();
        var player = Player.main;
        player.transform.parent = null;
        MainCameraControl.main.lookAroundMode = false;
        var chair = player.currChair;
        chair.Subscribe(player, false);
        chair.currentPlayer = null;
        chair.releaseCinematicController.SkipCinematic(player);
        chair.releaseCinematicController.animator.SetBool(chair.releaseCinematicController.animParam, true);
        Player.main.armsController.SetWorldIKTarget(null, null);
        UWE.Utils.GetEntityRoot(chair.gameObject).BroadcastMessage("StopPiloting");

        player.currentSub.GetComponent<SubControl>().Set(SubControl.Mode.GameObjects);
        player.mode = Player.Mode.Normal;
        player.currChair = null;
        player.playerModeChanged.Trigger(player.mode);
        UWE.CoroutineHost.StartCoroutine(ResetAnimParam(chair));
    }

    private IEnumerator ResetAnimParam(PilotingChair chair)
    {
        yield return new WaitForEndOfFrame();
        
        chair.releaseCinematicController.animator.SetBool(chair.releaseCinematicController.animParam, false);
    }

    public void OnProtoSerializeObjectTree(ProtobufSerializer serializer) { }

    public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
    {
        UWE.CoroutineHost.StartCoroutine(InitializeVehicle());
    }

    public void OnProtoSerialize(ProtobufSerializer serializer) { }

    public void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        ignoreCinematicStart.enabled = true;
    }

    private void OnSaveChanged(bool started)
    {
        if (!dockingBay.dockedVehicle) return;
        
        dockingBay.dockedVehicle.gameObject.SetActive(started);
    }

    private void OnEnable()
    {
        SaveLoadManager.notificationSaveInProgress += OnSaveChanged;
    }
    
    private void OnDisable()
    {
        SaveLoadManager.notificationSaveInProgress += OnSaveChanged;
    }


    public VehicleDockingBay GetDockingBay() => dockingBay;
}