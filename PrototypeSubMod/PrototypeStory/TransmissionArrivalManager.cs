using System;
using System.Collections;
using PrototypeSubMod.MiscMonobehaviors;
using PrototypeSubMod.Registration;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory;

public class TransmissionArrivalManager : MonoBehaviour
{
    [SerializeField] private VoiceNotificationManager voiceNotificationManager;
    [SerializeField] private VoiceNotification siteEntryVoiceline;

    private void Start()
    {
        IncreaseSizeOnBiomeEnter.OnBiomeSizeChanged += OnBiomeSizeChanged;
    }

    private void OnBiomeSizeChanged((string biome, bool isScaledUp) biomeData)
    {
        if (biomeData.biome != BiomeRegisterer.TransmissionSiteBiome) return;
        
        if (!biomeData.isScaledUp) return;

        StartCoroutine(PlayVoicelineDelayed());
    }

    private IEnumerator PlayVoicelineDelayed()
    {
        yield return new WaitForSeconds(5f);
        
        voiceNotificationManager.PlayVoiceNotification(siteEntryVoiceline, false);
    }

    private void OnDestroy()
    {
        IncreaseSizeOnBiomeEnter.OnBiomeSizeChanged -= OnBiomeSizeChanged;
    }
}