using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class LaserFireShot : CinematicShot
{
    public override event Action<DeviceCinematicManager> OnShotCompleted;

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

    public void FireLaser()
    {
        deviceCinematicManager.FireLaser();
    }
}