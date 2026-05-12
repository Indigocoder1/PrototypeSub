using System;
using System.Collections.Generic;
using System.Linq;
using PrototypeSubMod.MiscMonobehaviors;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class PlanetViewShot : CinematicShot
{
    public override event Action<DeviceCinematicManager> OnShotCompleted;
    
    [SerializeField] private QepLaserSpawner laserSpawner;
    [SerializeField] private Spawn4546B planetSpawner;

    private List<WaterscapeVolume> volumes;

    private void Start()
    {
        planetSpawner.SpawnPlanet();
    }

    public override void PlayShot(Animator animator, DeviceCinematicManager cinematicManager)
    {
        base.PlayShot(animator, cinematicManager);
        volumes = FindObjectsOfType<WaterscapeVolume>().ToList();
        SetFogActive(false);
    }

    public void SpawnPlanetLaser()
    {
        laserSpawner.PlayLaserVFX();
    }
    
    public void EndPlanetViewShot()
    {
        laserSpawner.DeleteEffects();
        OnShotCompleted?.Invoke(deviceCinematicManager);
        SetFogActive(true);
    }

    private void SetFogActive(bool active)
    {
        foreach (var volume in volumes)
        {
            volume.enabled = active;
        }
    }
}