using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingRoomTeleporterManager : MonoBehaviour
{
    [SerializeField] private Camera previewCamera;
    [SerializeField] private LinkedTeleporter[] linkedTeleporters;

    [SerializeField, HideInInspector]
    public BearingTeleporterDoor[] doorsA;
    [SerializeField, HideInInspector]
    public BearingTeleporterDoor[] doorsB;

    private Dictionary<BearingTeleporterDoor, BearingTeleporterDoor> bearingTeleporterDoors = new();
    
    private void OnValidate()
    {
        doorsA = new BearingTeleporterDoor[linkedTeleporters.Length];
        doorsB = new BearingTeleporterDoor[linkedTeleporters.Length];

        for (int i = 0; i < linkedTeleporters.Length; i++)
        {
            doorsA[i] = linkedTeleporters[i].doorA;
            doorsB[i] = linkedTeleporters[i].doorB;
        }
    }

    private void Start()
    {
        for (int i = 0; i < doorsA.Length; i++)
        {
            var doorA = doorsA[i];
            var doorB = doorsB[i];

            SetPreviewImage(doorA, doorB);
            SetPreviewImage(doorB, doorA);
            
            bearingTeleporterDoors.Add(doorA, doorB);
            bearingTeleporterDoors.Add(doorB, doorA);
            doorA.onTryTeleport += OnTryTeleport;
            doorB.onTryTeleport += OnTryTeleport;
        }

        // Clear for memory
        doorsA = null;
        doorsB = null;
    }

    private void SetPreviewImage(BearingTeleporterDoor doorFrom, BearingTeleporterDoor doorTo)
    {
        var cameraPos = doorTo.GetTeleportPreviewPos();
        previewCamera.transform.position = cameraPos;
        previewCamera.transform.LookAt(doorTo.GetTeleportPreviewPos() - doorTo.GetTeleportPreviewLookPos());
        //previewCamera.projectionMatrix =
            //doorFrom.GetPreviewProjectionMatrix(cameraPos, previewCamera.nearClipPlane, previewCamera.farClipPlane, out var newNearClipPlane);
        
        previewCamera.Render();
        doorFrom.SetTeleporterPreview(previewCamera.targetTexture);
    }

    private void OnTryTeleport(BearingTeleporterDoor doorFrom)
    {
        if (!bearingTeleporterDoors.TryGetValue(doorFrom, out var doorTo))
        {
            throw new Exception($"Pair for {doorFrom} not found in teleporter doors mapping!");
        }

        var teleportToPosition = doorTo.GetTeleportInPosition();
        var teleportFromPosition = doorFrom.GetTeleportInPosition();
        var positionDelta = teleportToPosition.position - Player.main.transform.position;
        var localPlayerDir = teleportFromPosition.InverseTransformDirection(Player.main.transform.eulerAngles);
        var rotationDelta = teleportFromPosition.TransformDirection(-localPlayerDir) - localPlayerDir;

        var localVelocity = teleportFromPosition.InverseTransformVector(Player.main.rigidBody.velocity);
        var newVelocity = teleportToPosition.TransformVector(-localVelocity);
        var underwaterMotor = Player.main.playerController.underWaterController as UnderwaterMotor;
        
        Player.main.transform.eulerAngles += rotationDelta;
        Player.main.rigidBody.velocity = newVelocity;
        underwaterMotor.vel = newVelocity;
        underwaterMotor.previousVelocity = newVelocity;
        Player.main.SetPosition(Player.main.transform.position + positionDelta);
    }
}

[Serializable]
public class LinkedTeleporter
{
    public string name;
    public BearingTeleporterDoor doorA;
    public BearingTeleporterDoor doorB;
}