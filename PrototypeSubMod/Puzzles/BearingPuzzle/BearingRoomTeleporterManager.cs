using System;
using System.Collections;
using System.Collections.Generic;
using PrototypeSubMod.MiscMonobehaviors;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingRoomTeleporterManager : MonoBehaviour
{
    [SerializeField] private Camera previewCamera;
    [SerializeField] private LinkedTeleporter[] linkedTeleporters;
    [SerializeField] private FMODAsset teleportSfx;

    private RenderTexture cameraRenderTexture;
    
    [SerializeField, HideInInspector]
    public BearingTeleporterDoor[] doorsFrom;
    [SerializeField, HideInInspector]
    public BearingTeleporterDoor[] doorsTo;

    private Dictionary<BearingTeleporterDoor, BearingTeleporterDoor> bearingTeleporterDoors = new();
    
    private void OnValidate()
    {
        doorsFrom = new BearingTeleporterDoor[linkedTeleporters.Length];
        doorsTo = new BearingTeleporterDoor[linkedTeleporters.Length];

        for (int i = 0; i < linkedTeleporters.Length; i++)
        {
            doorsFrom[i] = linkedTeleporters[i].doorFrom;
            doorsTo[i] = linkedTeleporters[i].doorTo;
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
        
        for (int i = 0; i < doorsFrom.Length; i++)
        {
            var doorFrom = doorsFrom[i];
            var doorTo = doorsTo[i];
            
            bearingTeleporterDoors.Add(doorFrom, doorTo);
            doorFrom.onTryTeleport += OnTryTeleport;
        }

        // Clear for memory
        doorsFrom = null;
        doorsTo = null;

        StartCoroutine(GeneratePreviewsDelayed());
    }

    public void LinkTeleporters(BearingTeleporterDoor doorFrom, BearingTeleporterDoor doorTo)
    {
        bearingTeleporterDoors[doorFrom] = doorTo;
    }
    
    private IEnumerator GeneratePreviewsDelayed()
    {
        var lwe = GetComponentInParent<LargeWorldEntity>();
        yield return new WaitUntil(() => lwe.fadeTime > 0.5f);

        foreach (var item in bearingTeleporterDoors)
        {
            item.Key.SetRenderersActive(false);
            item.Value.SetRenderersActive(false);
        }
        
        foreach (var item in bearingTeleporterDoors)
        {
            GeneratePreviewImage(item.Key, item.Value);
        }
        
        foreach (var item in bearingTeleporterDoors)
        {
            item.Key.SetRenderersActive(true);
            item.Value.SetRenderersActive(true);
        }
    }

    public void GeneratePreviewImage(BearingTeleporterDoor doorFrom, BearingTeleporterDoor doorTo)
    {
        var playerRends = Player.main.GetComponentsInChildren<Renderer>();
        Dictionary<Renderer, bool> rendererStates = new();
        foreach (var rend in playerRends)
        {
            rendererStates.Add(rend, rend.enabled);
            rend.enabled = false;
        }
        
        var cameraPos = doorTo.GetTeleportPreviewPos();
        previewCamera.transform.position = cameraPos;
        previewCamera.transform.LookAt(doorTo.GetTeleportPreviewLookPos());
        
        previewCamera.Render();
        doorFrom.SetTeleporterPreview(previewCamera.targetTexture);
        
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
        var rotationDelta = teleportToPosition.eulerAngles - (teleportFromPosition.eulerAngles - new Vector3(0, 180, 0));
        var camera = MainCameraControl.main.transform;
        
        var localVelocity = teleportFromPosition.InverseTransformVector(-Player.main.rigidBody.velocity);
        var newVelocity = teleportToPosition.TransformVector(localVelocity);
        var underwaterMotor = Player.main.playerController.underWaterController as UnderwaterMotor;
        
        var newAngles = camera.eulerAngles + rotationDelta;
        MainCameraControl.main.rotationX += rotationDelta.y;
        camera.eulerAngles = newAngles;
        Player.main.rigidBody.velocity = newVelocity;
        underwaterMotor.vel = newVelocity;
        underwaterMotor.previousVelocity = newVelocity;
        Player.main.SetPosition(Player.main.transform.position + positionDelta);

        FMODUWE.PlayOneShot(teleportSfx, Player.main.transform.position, 0.5f);
        InterfloorTeleporter.PlayTeleportEffect(0.2f);
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
    public BearingTeleporterDoor doorFrom;
    public BearingTeleporterDoor doorTo;
}