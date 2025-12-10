using System;
using UnityEngine;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingTeleporterDoor : MonoBehaviour
{
    public event Action<BearingTeleporterDoor> onTryTeleport;
    
    [SerializeField] private Transform teleportInPosition;
    [SerializeField] private Renderer teleporterPreview;
    [SerializeField] private Transform nearPlaneA;
    [SerializeField] private Transform nearPlaneB;
    [SerializeField] private Transform nearPlaneC;

    private Renderer[] renderers;

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        teleporterPreview.gameObject.SetActive(false);
    }

    public void TeleportPlayer()
    {
        ErrorMessage.AddError("Teleporting player");
        onTryTeleport?.Invoke(this);
    }

    public void SetTeleporterPreview(Texture texture)
    {
        teleporterPreview.material.SetTexture("_MainTex", texture);
        teleporterPreview.gameObject.SetActive(true);
    }

    public void SetRenderersActive(bool active)
    {
        foreach (var rend in renderers)
        {
            rend.enabled = active;
        }
    }

    public Matrix4x4 GetPreviewProjectionMatrix(Vector3 cameraPos, float nearPlaneDist, float farPlaneDist, out float nearClipPlane)
    {
        var rightVector = (nearPlaneB.position - nearPlaneA.position).normalized;
        var upVector = (nearPlaneC.position - nearPlaneA.position).normalized;
        var normalVector = Vector3.Cross(upVector, rightVector).normalized;

        var va = nearPlaneA.position - cameraPos;
        var vb = nearPlaneB.position - cameraPos;
        var vc = nearPlaneC.position - cameraPos;

        var distance = -Vector3.Dot(va, normalVector);

        var nd = nearPlaneDist / distance;
        var l = Vector3.Dot(rightVector, va) * nd;
        var r = Vector3.Dot(rightVector, vb) * nd;
        var b = Vector3.Dot(upVector, va) * nd;
        var t = Vector3.Dot(upVector, vc) * nd;

        nearClipPlane = distance;
        var projectionMatrix = Matrix4x4.Frustum(l, r, b, t, nd, farPlaneDist);
        return projectionMatrix;
    }

    public Transform GetTeleportInPosition() => teleportInPosition;
    public Vector3 GetTeleportPreviewPos() => teleporterPreview.transform.position + teleporterPreview.transform.up * 2;
    public Vector3 GetTeleportPreviewLookPos() => teleporterPreview.transform.position;
}