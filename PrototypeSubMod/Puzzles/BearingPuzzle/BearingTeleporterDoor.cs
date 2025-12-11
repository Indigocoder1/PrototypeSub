using System;
using UnityEngine;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingTeleporterDoor : MonoBehaviour
{
    public event Action<BearingTeleporterDoor> onTryTeleport;
    
    [SerializeField] private Transform teleportInPosition;
    [SerializeField] private Renderer teleporterPreview;

    private Renderer[] childRenderers;
    
    private RenderTexture previewTexture;
    
    private void Awake()
    {
        teleporterPreview.gameObject.SetActive(false);
        previewTexture = new RenderTexture(1024, 1024, 0);
        childRenderers = GetComponentsInChildren<Renderer>();
    }

    public void TeleportPlayer()
    {
        onTryTeleport?.Invoke(this);
    }

    public void SetTeleporterPreview(Texture texture)
    {
        teleporterPreview.material.EnableKeyword("MARMO_EMISSION");
        
        teleporterPreview.gameObject.SetActive(true);
        Graphics.Blit(texture, previewTexture);
        teleporterPreview.material.SetTexture(ShaderPropertyID._MainTex, previewTexture);
        teleporterPreview.material.SetTexture(ShaderPropertyID._SpecTex, previewTexture);
        teleporterPreview.material.SetTexture(ShaderPropertyID._Illum, previewTexture);
        teleporterPreview.material.SetFloat("_GlowStrength", 0.5f);
        teleporterPreview.material.SetFloat("_GlowStrengthNight", 0.5f);
    }

    public void SetRenderersActive(bool active)
    {
        foreach (var rend in childRenderers)
        {
            rend.enabled = active;
        }
    }

    private void OnDestroy()
    {
        previewTexture.Release();
        Destroy(previewTexture);
    }

    public Transform GetTeleportInPosition() => teleportInPosition;
    public Vector3 GetTeleportPreviewPos() => teleportInPosition.transform.position - Vector3.up;
    public Vector3 GetTeleportPreviewLookPos() => teleportInPosition.transform.position + teleportInPosition.transform.forward - Vector3.up;
}