using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nautilus.Extensions;
using Nautilus.Handlers;
using Nautilus.Utility;
using PrototypeSubMod.Patches;
using PrototypeSubMod.PrecursorWearables;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.Prefabs.Factors;
using PrototypeSubMod.Registration;
using SubLibrary.Handlers;
using Unity.Audio;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.PostProcessing;

namespace PrototypeSubMod.Factors.Locator;

public class Locator : Factor
{
    public Locator()
    {
        cooldown = 1f;
        duration = 10f; // keep synced with actual length of `FacilityDetectionPing.wav`
    }

    private GameObject locatorSource;

    // private void Awake()
    // {
    //     pingSfxAsset = AudioUtils.GetFmodAsset("event:/sub/cyclops/ping");
    // }

    public override GameInput.Button GetUseButton() => InputRegisterer.LocatorButton;
    public override void OnEquipped()
    {
        base.OnEquipped();
        if (locatorSource is null)
            UWE.CoroutineHost.StartCoroutine(SpawnLocatorSource());
        else
            locatorSource.SetActive(true);
    }

    public override void OnUnequipped()
    {
        base.OnUnequipped();
        locatorSource?.SetActive(false);
    }

    private IEnumerator SpawnLocatorSource()
    {
        var prefabTask = CraftData.GetPrefabForTechTypeAsync(LocatorFactorSource.prefabInfo.TechType);
        yield return prefabTask;
        var prefab = prefabTask.GetResult();
        locatorSource = Instantiate(prefab);
        locatorSource.transform.SetParent(Player.main.transform);
        locatorSource.transform.localPosition = new Vector3(0, 0, 0);
        Debug.Log($"Locator source spawned. Parent: {locatorSource.transform.parent.name}");
        var hasComponent = locatorSource.TryGetComponent<LocatorSourceManager>(out var lsm);
        Debug.Log($"Locator source has LocatorSourceManager component: {hasComponent}");
        lsm.locatorFactor = this;
    }

    // private GameObject CreateLocatorEffect()
    // {
    //     var locatorSource = new GameObject("LocatorSource");
    //     locatorSource.transform.SetParent(Player.main.transform);
    //     locatorSource.transform.localPosition = new Vector3(0, 0, 0);
    //     srcSfx = locatorSource.AddComponent<FMOD_CustomEmitter>();
    //     foreach (var field in typeof(FMOD_CustomEmitter).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
    //         field.SetValue(srcSfx, field.GetValue(pingSfx));
    //     return locatorSource;
    // }


    public override void StartUse()
    {
        base.StartUse();
        if (locatorSource is null) UWE.CoroutineHost.StartCoroutine(SpawnLocatorSource());
        locatorSource?.GetComponent<LocatorSourceManager>().StartUse();
    }

//   public override void UpdateFactor()
//   {
//     base.UpdateFactor();
//   }

    public override void StopUse()
    {
        // locatorSource?.GetComponent<LocatorSourceManager>().GracefulStop();
        base.StopUse();
    }
}
