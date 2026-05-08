using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class EnergyTransferShot : CinematicShot
{
    public override event Action<DeviceCinematicManager> OnShotCompleted;

    [SerializeField] private GameObject lightningObjects;
    [SerializeField] private PrefabSpawn[] impactFrameSpawners;

    private void Start()
    {
        lightningObjects.SetActive(false);
    }

    public void EndEnergyTransferShot()
    {
        OnShotCompleted?.Invoke(deviceCinematicManager);
    }

    public void EnableLightningObjects()
    {
        lightningObjects.SetActive(true);
    }
    
    public void DisableLightningObjects()
    {
        lightningObjects.SetActive(false);
    }

    public void SpawnImpactFrames()
    {
        foreach (var frameSpawner in impactFrameSpawners)
        {
            frameSpawner.SpawnManual();
        }
    }
}