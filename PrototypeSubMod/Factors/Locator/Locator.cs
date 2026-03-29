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
using Story;
using SubLibrary.Handlers;
using Unity.Audio;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.PostProcessing;

namespace PrototypeSubMod.Factors.Locator;

public class Locator : Factor
{
    private bool toggle = true;
    private float timeLastPing;
    [SerializeField] private int maxPingDistance = 300;
    [SerializeField] private FMODAsset pingSfxAsset;

    public Locator()
    {
        duration = 7.0759f; // keep synced with actual length of `FacilityDetectionPing.wav` in seconds
    }

    private GameObject locatorSource;

    public override GameInput.Button GetUseButton() => InputRegisterer.LocatorButton;
    public override void OnEquipped()
    {
        base.OnEquipped();
        timeLastPing = Time.time - duration;
        
        if (StoryGoalManager.main.IsGoalComplete("ProtoLocatorEquipped")) return;

        StoryGoalManager.main.OnGoalComplete("ProtoLocatorEquipped");
        var text = Language.main.GetFormat("ProtoLocatorHint", GameInput.FormatButton(GetUseButton()));
        Hint.main.message.SetText(text);
        Hint.main.message.Show();
    }

    public override void StartUse()
    {
        base.StartUse();
        // Actually called every time the use button is pressed, so toggle
        toggle = !toggle;

        if (toggle)
        {
            FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("ButtonSelect"), Player.main.transform.position);
            ErrorMessage.AddError(Language.main.Get("ProtoLocatorEnabled"));
        }
        else
        {
            FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("ButtonBack"), Player.main.transform.position);
            ErrorMessage.AddError(Language.main.Get("ProtoLocatorDisabled"));
        }
    }

    public override void UpdateFactor()
    {
        base.UpdateFactor();
        if (!toggle || Time.time - timeLastPing < duration) return;
        timeLastPing = Time.time;
        
        var loc = Player.main.transform.position;

        (string name, Vector3 pos, float dist) = Plugin.FACILITY_POSITIONS
            .Select(kv => (kv.Key, kv.Value, dist: Vector3.Distance(loc, kv.Value)))
            .OrderBy(t => t.dist)
            .FirstOrFallback(("Player", loc, 0f));

        // maxPingDistance should be about equal to the FMOD asset's 3D max distance to prevent popping sound, but I can't reference for some reason, so just hardcode it here for now -Chris
        if (dist > maxPingDistance) return;
        if (dist == 0f) return;
        Utils.PlayFMODAsset(pingSfxAsset, pos);
    }
}
