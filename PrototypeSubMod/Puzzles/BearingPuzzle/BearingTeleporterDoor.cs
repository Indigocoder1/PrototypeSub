using System;
using UnityEngine;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingTeleporterDoor : MonoBehaviour
{
    public event Action<BearingTeleporterDoor> onTryTeleport;
    
    [SerializeField] private Transform teleportInPosition;
    [SerializeField] private Renderer teleporterPreview;

    private float blurThreshold = 4f;
    private float maxBlur = 2f;

    private Renderer[] childRenderers;
    private RadiationsScreenFXController radiationController;
    private RenderTexture previewTexture;
    private Color previousRadiationColor;
    private bool wasHandlingBlur;
    
    private void Awake()
    {
        teleporterPreview.gameObject.SetActive(false);
        previewTexture = new RenderTexture(1024, 1024, 0);
        childRenderers = GetComponentsInChildren<Renderer>();
        radiationController = Camera.main.GetComponent<RadiationsScreenFXController>();
    }

    public void TeleportPlayer()
    {
        onTryTeleport?.Invoke(this);
    }

    private void Update()
    {
        HandleBlurVFX(out var doingBlur);

        if (doingBlur && !wasHandlingBlur)
        {
            previousRadiationColor = radiationController.fx.color;
            radiationController.enabled = false;
            radiationController.fx.enabled = true;
            radiationController.fx.color = new Color(0.125f, 0.175f, 0.125f, 1f);
        }

        if (!doingBlur && wasHandlingBlur)
        {
            radiationController.enabled = true;
            radiationController.fx.enabled = false;
            radiationController.fx.noiseFactor = 0;
            radiationController.fx.color = previousRadiationColor;
        }
        
        wasHandlingBlur = doingBlur;
    }

    private void HandleBlurVFX(out bool doingBlur)
    {
        var distToPlayer = Vector3.Distance(teleportInPosition.position, Player.main.transform.position);
        doingBlur = false;
        if (distToPlayer > blurThreshold)
        {
            return;
        }

        if (!radiationController.fx.enabled)
        {
            radiationController.enabled = false;
            radiationController.fx.enabled = true;
            radiationController.fx.color = new Color(0.125f, 0.175f, 0.125f, 1f);
        }

        radiationController.fx.noiseFactor = Mathf.Lerp(maxBlur, 0, distToPlayer / blurThreshold);
        doingBlur = true;
    }
    
    private float CubicOut(float start, float end, float time)
    {
        return start + (end - start) * (1 - Mathf.Pow(1 - time, 4));
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