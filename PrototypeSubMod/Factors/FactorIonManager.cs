using System;
using Nautilus.Json;
using UnityEngine;

namespace PrototypeSubMod.Factors;

public class FactorIonManager : MonoBehaviour
{
    private float maxIonEnergy = 100;

    private PrefabIdentifier prefabIdentifier;
    private float currentIonEnergy;

    private void Awake()
    {
        prefabIdentifier = GetComponent<PrefabIdentifier>();
        
        if (Plugin.GlobalSaveData.suitIonEnergies.TryGetValue(prefabIdentifier.Id, out var charge))
        {
            currentIonEnergy = charge;
        }
        else
        {
            currentIonEnergy = maxIonEnergy;
        }

        Plugin.GlobalSaveData.OnStartedSaving += OnBeforeSave;
    }

    public bool ConsumeEnergy(float energy)
    {
        currentIonEnergy = Mathf.Max(0, currentIonEnergy - energy);
        return currentIonEnergy > 0;
    }
    
    public void AddEnergy(float energy)
    {
        currentIonEnergy = Mathf.Min(maxIonEnergy, currentIonEnergy - energy);
    }

    public float GetCurrentEnergy() => currentIonEnergy;
    public float GetNormalizedCharge() => currentIonEnergy / maxIonEnergy;

    private void OnBeforeSave(object sender, JsonFileEventArgs args)
    {
        Plugin.GlobalSaveData.suitIonEnergies[prefabIdentifier.Id] = currentIonEnergy;
    }

    private void OnDestroy()
    {
        Plugin.GlobalSaveData.OnStartedSaving -= OnBeforeSave;
    }
}