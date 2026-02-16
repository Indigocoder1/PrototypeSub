using System;
using System.Collections;
using System.Collections.Generic;
using PrototypeSubMod.MiscMonobehaviors.PrefabRetrievers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingTeleporterDoor : MonoBehaviour
{
    public event Action<BearingTeleporterDoor> onTryTeleport;
    
    [SerializeField] private Transform teleportInPosition;
    [SerializeField] private Renderer teleporterPreview;
    [SerializeField] private bool blurOnApproach = true;

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
        UWE.CoroutineHost.StartCoroutine(SpawnParticles());
    }

    private IEnumerator SpawnParticles()
    {
        yield return new WaitForSeconds(Random.Range(0f, 1f));
        SpawnParticles(Vector3.zero);
        yield return new WaitForSeconds(Random.Range(0f, 0.5f));
        SpawnParticles(new Vector3(0, 4, 0));
    }

    private void SpawnParticles(Vector3 offset)
    {
        var particles = new GameObject("Particles");
        particles.transform.SetParent(teleporterPreview.transform);
        particles.transform.localPosition = new Vector3(0, 1.5f, 10f) + offset;
        particles.transform.localScale = new Vector3(5, 4, 0.5f);
        particles.transform.localEulerAngles = new Vector3(90, 180, 0);
        var fxSpawner = particles.AddComponent<SpawnTerminalFX>();
        fxSpawner.SetRemoveFXPaths("x_Precursor_ComputerTerminal_SmallSymbol", "x_Precursor_ComputerTerminal_Symbol", "Light_Under", "x_Precursor_ComputerTerminal_Halo");
    }

    public void TeleportPlayer()
    {
        onTryTeleport?.Invoke(this);
    }

    private void Update()
    {
        if (!blurOnApproach) return;
        
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
    public Vector3 GetTeleportPreviewPos() => teleportInPosition.transform.position + Vector3.down;
    public Vector3 GetTeleportPreviewLookPos() => teleportInPosition.transform.position + teleportInPosition.transform.forward + Vector3.down;
}