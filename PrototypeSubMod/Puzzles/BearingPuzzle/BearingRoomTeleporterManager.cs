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

        UWE.CoroutineHost.StartCoroutine(GeneratePreviewsDelayed());
    }

    public void LinkTeleporters(BearingTeleporterDoor doorFrom, BearingTeleporterDoor doorTo)
    {
        bearingTeleporterDoors[doorFrom] = doorTo;
    }
    
    private IEnumerator GeneratePreviewsDelayed()
    {
        var lwe = GetComponentInParent<LargeWorldEntity>();
        if (lwe != null)
        {
            yield return new WaitUntil(() => lwe.fadeTime > 0.5f);
        }
        else
        {
            yield return null;
            yield return new WaitUntil(() => gameObject.activeInHierarchy);
        }

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

        StartCoroutine(TeleportDelayed(doorFrom, doorTo));
    }

    private IEnumerator TeleportDelayed(BearingTeleporterDoor doorFrom, BearingTeleporterDoor doorTo)
    {
        InterfloorTeleporter.PlayTeleportEffect(0.2f);
        var teleportToPosition = doorTo.GetTeleportInPosition();
        var teleportFromPosition = doorFrom.GetTeleportInPosition();
        var positionDelta = teleportToPosition.position - teleportFromPosition.position;
        var rotationDelta = teleportToPosition.eulerAngles - (teleportFromPosition.eulerAngles - new Vector3(0, 180, 0));
        var camera = MainCameraControl.main.transform;
        
        var localVelocity = teleportFromPosition.InverseTransformVector(-Player.main.playerController.velocity);
        var newVelocity = teleportToPosition.TransformVector(localVelocity);
        var underwaterMotor = Player.main.playerController.underWaterController as UnderwaterMotor;
        
        var newAngles = camera.eulerAngles + rotationDelta;
        
        yield return new WaitForSeconds(0.1f);
        FMODUWE.PlayOneShot(teleportSfx, Player.main.transform.position, 0.5f);

        var oldVelocity = Player.main.playerController.velocity;
        var oldForward = Player.main.transform.forward; 
        MainCameraControl.main.rotationX += rotationDelta.y;
        camera.eulerAngles = newAngles;
        Player.main.playerController.groundController.SetVelocity(newVelocity);
        underwaterMotor.vel = newVelocity;
        underwaterMotor.previousVelocity = newVelocity;

        var velocityOffset = oldVelocity *
                             ((1 - Mathf.Abs(Vector3.Dot(oldForward, teleportToPosition.forward))) * 3f);
        Plugin.Logger.LogInfo(
            $"Velocity offset = {velocityOffset} | Dot = {Vector3.Dot(Player.main.transform.forward, teleportToPosition.forward)} | Velocity magnitude = {oldVelocity.magnitude}");
        Player.main.SetPosition(Player.main.transform.position + positionDelta);
        
        var originalPos = Player.main.transform.position + teleportToPosition.forward * 0.5f;

        yield break;
        Player.main.playerController.enabled = false;
        // Wait until a FixedUpdate has occurred
        var timestepIncrements = (int)(Time.fixedUnscaledTime / Time.fixedUnscaledDeltaTime);
        while (timestepIncrements == (int)(Time.fixedUnscaledTime / Time.fixedUnscaledDeltaTime))
        {
            var offset = Vector3.Project(Player.main.transform.position - originalPos, teleportToPosition.forward);
            Player.main.SetPosition(originalPos);
            Player.main.playerController.velocity = Vector3.zero;
            yield return null;
        }
        Player.main.playerController.enabled = true;
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