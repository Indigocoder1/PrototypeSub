using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class LaserFireShot : CinematicShot
{
    public override event Action<DeviceCinematicManager> OnShotCompleted;

    [SerializeField] private float emissionPulseDuration;
    [SerializeField] private float emissionIntensityMultiplier;
    [SerializeField] private PrefabSpawn impactFrame;
    
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
}