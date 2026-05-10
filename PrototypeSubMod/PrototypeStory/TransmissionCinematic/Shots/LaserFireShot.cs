using System;
using PrototypeSubMod.VehicleAccess;
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

    public void SpawnLaserImpactFrame()
    {
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
        OnShotCompleted?.Invoke(deviceCinematicManager);
    }

    public void PlayWarmupVFX() => laserSpawner.PlayWarmupVFX();
    public void StopWarmupVFX() => laserSpawner.StopWarmupVFX();
    public void PlayMuzzleVFX() => laserSpawner.PlayMuzzleVFX();
    public void PlayLaserVFX() => laserSpawner.PlayLaserVFX();
}