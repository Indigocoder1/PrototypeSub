using System;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace PrototypeSubMod.MiscMonobehaviors;

public class PostProcessOverride : MonoBehaviour
{
    public static event Action OnBeforeOverrideApplied;
    
    [SerializeField] private CameraPostProcessApplier postProcessApplier;
    [SerializeField] private bool applyOnStart;
    [SerializeField] private BloomModel bloomOverride;

    private BloomModel originalBloom;

    private void Start()
    {
        OnBeforeOverrideApplied += ResetEffects;
        
        if (!applyOnStart) return;

        ApplyOverrides();
    }

    public void ApplyOverrides()
    {
        if (!postProcessApplier.GetComponentsAdded())
        {
            postProcessApplier.OnComponentsAdded += ApplyEffects;
            return;
        }

        ApplyEffects();
    }

    private void ApplyEffects()
    {
        OnBeforeOverrideApplied?.Invoke();
        originalBloom = UwePostProcessingManager.currentProfile.bloom;
        UwePostProcessingManager.currentProfile.bloom = bloomOverride;
    }

    public void ResetEffects()
    {
        if (originalBloom == null) return;

        Plugin.Logger.LogInfo($"Resetting bloom settings");
        UwePostProcessingManager.currentProfile.bloom = originalBloom;
    }

    private void OnDestroy()
    {
        ResetEffects();
        OnBeforeOverrideApplied -= ResetEffects;
    }
}