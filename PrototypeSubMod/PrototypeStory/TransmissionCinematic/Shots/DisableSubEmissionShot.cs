using System;
using System.Collections;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using UnityEngine;
using UnityEngine.Serialization;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class DisableSubEmissionShot : CinematicShot
{
    public override event Action<DeviceCinematicManager> OnShotCompleted;
    
    [FormerlySerializedAs("emissionAreaProgress")] [SerializeField] private EmissionAreaManager emissionAreaManager;
    [SerializeField] private float timeToDisableEmission;
    [SerializeField] private AnimationCurve progressOverTime;
    [SerializeField] private AnimationCurve intensityOverTime;
    
    [Header("SFX")]
    [SerializeField] private FMOD_CustomEmitter emissionDisableSfx;
    [SerializeField] private FMOD_CustomEmitter devicePulseSfx;

    public override void PlayShot(Animator animator, DeviceCinematicManager cinematicManager)
    {
        emissionAreaManager.UpdateRenderers();
        StartCoroutine(DisableEmissionAsync());
        base.PlayShot(animator, cinematicManager);
    }

    private IEnumerator DisableEmissionAsync()
    {
        float currentTime = 0;
        while (currentTime < timeToDisableEmission)
        {
            var progress = currentTime / timeToDisableEmission;
            emissionAreaManager.SetValues(progressOverTime.Evaluate(progress), intensityOverTime.Evaluate(progress));
            currentTime += Time.deltaTime;
            yield return null;
        }
    }

    public void PlayTransferSfx()
    {
        emissionDisableSfx.Play();
    }

    public void PulseDeviceEmission()
    {
        deviceCinematicManager?.PulseEmission();
        devicePulseSfx.Play();
    }

    public void EndEmissionShot()
    {
        OnShotCompleted?.Invoke(deviceCinematicManager);
    }

    public void PrepFiringPos()
    {
        deviceCinematicManager.PrepFiringPos();
    }
}