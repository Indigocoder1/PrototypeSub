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
    [SerializeField] private FogModel fogOverride;

    private BloomModel originalBloom;
    private FogModel originalFog;

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
        originalFog = UwePostProcessingManager.currentProfile.fog;
        UwePostProcessingManager.currentProfile.bloom = bloomOverride;
        UwePostProcessingManager.currentProfile.fog = fogOverride;
    }

    public void ResetEffects()
    {
        if (originalBloom == null || originalFog == null) return;
        
        UwePostProcessingManager.currentProfile.bloom = originalBloom;
        UwePostProcessingManager.currentProfile.fog = originalFog;
    }

    private void OnDestroy()
    {
        ResetEffects();
        OnBeforeOverrideApplied -= ResetEffects;
    }
}