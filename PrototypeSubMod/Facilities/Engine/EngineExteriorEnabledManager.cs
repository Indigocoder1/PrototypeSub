using System;
using Nautilus.Json;
using PrototypeSubMod.Patches;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Engine;

public class EngineExteriorEnabledManager : MonoBehaviour
{
    [SerializeField] private GameObject disabledObjects;
    [SerializeField] private Collider facilityBounds;

    private void Start()
    {
        Plugin.GlobalSaveData.OnStartedSaving += SaveStatus;
        FreecamController_Patches.onExitFreecam += OnExitFreecam;

        disabledObjects.SetActive(!Plugin.GlobalSaveData.insideEngineFacility);
    }

    private void SaveStatus(object sender, JsonFileEventArgs args)
    {
        Plugin.GlobalSaveData.insideEngineFacility = !disabledObjects.activeSelf;
    }

    private void OnExitFreecam()
    {
        bool inBounds = facilityBounds.bounds.Contains(Player.main.transform.position);
        disabledObjects.SetActive(!inBounds);
        if (!inBounds)
        {
            Player.main.SetPrecursorOutOfWater(false);
        }
    }

    private void OnDestroy()
    {
        Plugin.GlobalSaveData.OnStartedSaving -= SaveStatus;
        FreecamController_Patches.onExitFreecam -= OnExitFreecam;
    }
}