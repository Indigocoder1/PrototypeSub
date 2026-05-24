using System;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class LaserFireShot : CinematicShot
{
    public override event Action<DeviceCinematicManager> OnShotCompleted;

    [SerializeField] private PrefabSpawn impactFrame;
    [SerializeField] private PrefabSpawn cinematicWorm;
    [SerializeField] private QepLaserSpawner laserSpawner;
    [SerializeField] private float emissionPulseDuration;
    [SerializeField] private float emissionIntensityMultiplier;
    
    [Header("SFX")]
    [SerializeField] private FMOD_CustomEmitter deviceChargeSfx;
    [SerializeField] private FMOD_CustomEmitter laserFireSfx;
    [SerializeField] private FMOD_CustomEmitter wyrmScreechSfx;

    public override void PlayShot(Animator animator, DeviceCinematicManager cinematicManager)
    {
        base.PlayShot(animator, cinematicManager);
        deviceChargeSfx.Play();
    }

    public void PulseDeviceEmissionLong()
    {
        deviceCinematicManager.PulseEmission(emissionPulseDuration, emissionIntensityMultiplier);
    }

    public void ChargeUpDevice()
    {
        deviceCinematicManager.ChargeLaser();
    }

    public void StartLaserFireAnim()
    {
        deviceCinematicManager.FireLaserAnim();
    }

    public void PlayWyrmScreechSfx()
    {
        wyrmScreechSfx.Play();
    }

    public void PlayLaserFireSfx()
    {
        laserFireSfx.Play();
    }

    public void SpawnLaserImpactFrame()
    {
        if (impactFrame.spawnedObj)
        {
            Destroy(impactFrame.spawnedObj);
            StartCoroutine(SpawnFrameAsync());
        }
        else
        {
            impactFrame.SpawnManual();
        }
    }

    private IEnumerator SpawnFrameAsync()
    {
        yield return null;
        impactFrame.SpawnManual();
    }

    public void SpawnCinematicWorm()
    {
        cinematicWorm.SpawnManual();
    }

    public void DestroyCinematicWorm()
    {
        Destroy(cinematicWorm.spawnedObj);
    }

    public void EndLaserFireShot()
    {
        laserSpawner.DeleteEffects();
        OnShotCompleted?.Invoke(deviceCinematicManager);
    }

    public void PlayWarmupVFX() => laserSpawner.PlayWarmupVFX();
    public void StopWarmupVFX() => laserSpawner.StopWarmupVFX();
    public void PlayMuzzleVFX() => laserSpawner.PlayMuzzleVFX();
    public void PlayLaserVFX() => laserSpawner.PlayLaserVFX();
}