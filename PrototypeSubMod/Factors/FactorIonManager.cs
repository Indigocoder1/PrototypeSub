using System;
using Nautilus.Json;
using UnityEngine;

namespace PrototypeSubMod.Factors;

public class FactorIonManager : MonoBehaviour, IProtoEventListener
{
    private float maxIonEnergy = 100;

    private PrefabIdentifier prefabIdentifier;
    private float currentIonEnergy;

    private void Awake()
    {
        prefabIdentifier = GetComponent<PrefabIdentifier>();

        Plugin.GlobalSaveData.OnStartedSaving += OnBeforeSave;
        TrySpawnResourceUI();

        currentIonEnergy = maxIonEnergy;
    }
    
    private void TrySpawnResourceUI()
    {
        var oxygenBar = uGUI.main.transform.Find("ScreenCanvas/HUD/Content/BarsPanel/OxygenBar");
        if (oxygenBar.Find("PrecursorSuitCharge") == null)
        {
            var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("PrecursorSuitCharge");
            var instance = Instantiate(prefab, oxygenBar);
            instance.name = "PrecursorSuitCharge";
            instance.transform.localPosition = new Vector3(0, 0, 0);
            instance.transform.localScale = Vector3.one * 0.7f;
            var hideForScreenshots = instance.EnsureComponent<HideForScreenshots>();
            hideForScreenshots.recursive = true;
        }
    }

    public bool ConsumeEnergy(float energy)
    {
        if (GameModeUtils.IsCheatActive(GameModeOption.NoEnergy))
        {
            return true;
        }
        
        currentIonEnergy = Mathf.Max(0, currentIonEnergy - energy);
        return currentIonEnergy > 0;
    }
    
    public void AddEnergy(float energy)
    {
        currentIonEnergy = Mathf.Min(maxIonEnergy, currentIonEnergy + energy);
    }

    public float GetCurrentEnergy() => currentIonEnergy;
    public float GetMaxEnergy() => maxIonEnergy;
    public float GetNormalizedCharge() => currentIonEnergy / maxIonEnergy;

    private void OnBeforeSave(object sender, JsonFileEventArgs args)
    {
        Plugin.GlobalSaveData.suitIonEnergies[prefabIdentifier.Id] = currentIonEnergy;
    }

    private void OnDestroy()
    {
        Plugin.GlobalSaveData.OnStartedSaving -= OnBeforeSave;
    }

    public void OnProtoSerialize(ProtobufSerializer serializer) { }

    public void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        if (Plugin.GlobalSaveData.suitIonEnergies.TryGetValue(prefabIdentifier.Id, out var charge))
        {
            currentIonEnergy = charge;
        }
        else
        {
            currentIonEnergy = maxIonEnergy;
        }
    }
}