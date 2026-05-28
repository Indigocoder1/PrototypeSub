using System;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace PrototypeSubMod.MiscMonobehaviors;

public class PostProcessOverride : MonoBehaviour
{
    [SerializeField] private CameraPostProcessApplier postProcessApplier;
    [SerializeField] private bool applyOnStart;
    [SerializeField] private BloomModel bloomOverride;

    private BloomModel originalBloom;

    private void Start()
    {
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
        ResetEffects();
        originalBloom = UwePostProcessingManager.currentProfile.bloom;
        UwePostProcessingManager.currentProfile.bloom = bloomOverride;
    }

    private void ResetEffects()
    {
        if (originalBloom == null) return;
        
        UwePostProcessingManager.currentProfile.bloom = originalBloom;
    }

    private void OnDestroy()
    {
        ResetEffects();
    }
}