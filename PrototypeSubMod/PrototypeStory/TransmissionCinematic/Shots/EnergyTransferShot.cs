using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class EnergyTransferShot : CinematicShot
{
    public override event Action OnShotCompleted;

    [SerializeField] private GameObject lightningObjects;
    [SerializeField] private PrefabSpawn[] impactFrameSpawners;

    private void Start()
    {
        lightningObjects.SetActive(false);
    }

    public void EndEnergyTransferShot()
    {
        OnShotCompleted?.Invoke();
    }

    public void EnableLightningObjects()
    {
        lightningObjects.SetActive(true);
    }

    public void SpawnImpactFrames()
    {
        foreach (var frameSpawner in impactFrameSpawners)
        {
            frameSpawner.SpawnManual();
        }
    }
}