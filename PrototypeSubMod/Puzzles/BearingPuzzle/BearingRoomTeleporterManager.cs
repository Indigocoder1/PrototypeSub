using System;
using System.Collections.Generic;
using PrototypeSubMod.MiscMonobehaviors;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingRoomTeleporterManager : MonoBehaviour
{
    [SerializeField] private Camera previewCamera;
    [SerializeField] private LinkedTeleporter[] linkedTeleporters;

    private RenderTexture cameraRenderTexture;
    
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

    private void Awake()
    {
        previewCamera.gameObject.EnsureComponent<CameraPostProcessApplier>();
    }

    private void Start()
    {
        if (cameraRenderTexture == null)
        {
            cameraRenderTexture = new RenderTexture(1024, 1024, 0, GraphicsFormat.R8G8B8A8_UNorm);
            cameraRenderTexture.Create();
            previewCamera.targetTexture = cameraRenderTexture;
        }
        
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
        var playerRends = Player.main.GetComponentsInChildren<Renderer>();
        Dictionary<Renderer, bool> rendererStates = new();
        foreach (var rend in playerRends)
        {
            rendererStates.Add(rend, rend.enabled);
            rend.enabled = false;
        }

        doorFrom.SetRenderersActive(false);
        doorTo.SetRenderersActive(false);
        
        var cameraPos = doorTo.GetTeleportPreviewPos();
        previewCamera.transform.position = cameraPos;
        previewCamera.transform.LookAt(doorTo.GetTeleportPreviewLookPos());
        
        previewCamera.Render();
        doorFrom.SetTeleporterPreview(previewCamera.targetTexture);
        doorFrom.SetRenderersActive(true);
        doorTo.SetRenderersActive(true);
        
        foreach (var rend in playerRends)
        {
            rend.enabled = rendererStates[rend];
        }
    }

    private void OnTryTeleport(BearingTeleporterDoor doorFrom)
    {
        if (!bearingTeleporterDoors.TryGetValue(doorFrom, out var doorTo))
        {
            throw new Exception($"Pair for {doorFrom} not found in teleporter doors mapping!");
        }

        var teleportToPosition = doorTo.GetTeleportInPosition();
        var teleportFromPosition = doorFrom.GetTeleportInPosition();
        var positionDelta = teleportToPosition.position - teleportFromPosition.position;
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

    private void OnDestroy()
    {
        if (cameraRenderTexture == null) return;
        
        cameraRenderTexture.Release();
        Destroy(cameraRenderTexture);
    }
}

[Serializable]
public class LinkedTeleporter
{
    public string name;
    public BearingTeleporterDoor doorA;
    public BearingTeleporterDoor doorB;
}