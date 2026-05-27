using System;
using System.Collections;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using Nautilus.Handlers;
using Nautilus.Utility;
using PrototypeSubMod.DestructionEvent;
using PrototypeSubMod.Facilities.Hull.WyrmActions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class WyrmSequenceMusic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WyrmFirstEncounterManager encounterManager;
    [SerializeField] private WyrmAction ramAction;
    [SerializeField] private WyrmShootTarget laserAction;
    
    [Header("Emitters")]
    [SerializeField] private FMOD_CustomEmitter introOneShot;
    [SerializeField] private FMOD_CustomEmitter dartActionLoop;
    [SerializeField] private FMOD_CustomEmitter laserActionLoop;
    [SerializeField] private FMOD_CustomEmitter intenseIntro;
    [SerializeField] private FMOD_CustomEmitter intense1;
    [SerializeField] private FMOD_CustomEmitter intense2;
    [SerializeField] private FMOD_CustomEmitter intense3;

    private FMOD_CustomEmitter currentSfxLoop;
    private FMOD_CustomEmitter nextLoopWanted;
    private List<FMOD_CustomEmitter> intenseLoops = new();
    private int intenseLoopIndex;
    private bool wyrmLaserAttacked;
    private bool eventRegistered;
    private bool stopSfx;
    
    private void Start()
    {
        if (encounterManager.FirstEncounterCompleted()) return;
        
        ramAction.OnActionComplete += OnRamHitTarget;
        laserAction.OnStartTargeting += OnLaserTargetingStart;
        laserAction.OnLaserImpact += OnLaserImpact;
        CalibrationRunManager.OnPointReached += OnReachNode;
        CalibrationRunManager.OnCalibrationCompleted += StopMusic;
        CalibrationRunManager.OnCalibrationFailed += StopMusic;
        ProtoDestructionEvent.OnSubDestroyed += OnSubDestroyed;
        eventRegistered = true;
        StartCoroutine(StartMusicAsync());
    }

    private IEnumerator StartMusicAsync()
    {
        yield return new WaitForSeconds(Random.Range(0, 0.2f));
        
        currentSfxLoop = introOneShot;
        nextLoopWanted = dartActionLoop;
        StartCoroutine(PlayLoops());
    }

    private IEnumerator PlayLoops()
    {
        CustomSoundHandler.TryGetCustomSoundChannel(currentSfxLoop.GetInstanceID(), out var channel);
        if (channel.isPlaying(out var playing) != RESULT.OK || !playing)
        {
            currentSfxLoop.Play();
        }

        CustomSoundHandler.TryGetCustomSoundChannel(currentSfxLoop.GetInstanceID(), out channel);

        channel.getCurrentSound(out var sound);
        sound.getLength(out var durationMS, TIMEUNIT.MS);
        var timeStarted = Time.time;
        
        yield return new WaitUntil(() =>
        {
            var timePassedCheck = (Time.time - timeStarted) * 1000 > durationMS;
            channel.isPlaying(out var isPlaying);
            var channelPlayingCheck = !isPlaying;
            return timePassedCheck || channelPlayingCheck;
        });

        if (nextLoopWanted != null && (intenseLoops.Count == 0 || !wyrmLaserAttacked))
        {
            currentSfxLoop = nextLoopWanted;
        }
        else if (intenseLoops.Count > 0 && wyrmLaserAttacked)
        {
            currentSfxLoop = intenseLoops[intenseLoopIndex];
            intenseLoopIndex = (intenseLoopIndex + 1) % intenseLoops.Count;
        }

        if (!stopSfx)
        {
            currentSfxLoop.Play();
            StartCoroutine(PlayLoops());
        }
        
        nextLoopWanted = null;
        stopSfx = false;
    }

    private void OnRamHitTarget()
    {
        nextLoopWanted = laserActionLoop;
        ramAction.OnActionComplete -= OnRamHitTarget;
    }

    private void OnLaserTargetingStart()
    {
        StopMusic();
        
        laserAction.OnStartTargeting -= OnLaserTargetingStart;
    }

    private void OnLaserImpact()
    {
        currentSfxLoop = intenseIntro;
        intenseLoops.Insert(0, intense1);
        stopSfx = false;
        wyrmLaserAttacked = true;
        StartCoroutine(PlayLoops());
        laserAction.OnLaserImpact -= OnLaserImpact;
    }
    
    private void OnReachNode(int point)
    {
        var addedLoop = point switch
        {
            1 => intense2,
            3 => intense3,
            _ => null
        };
        
        if (addedLoop == null) return;

        intenseLoops.Add(addedLoop);
    }

    private void StopMusic()
    {
        stopSfx = true;
        currentSfxLoop.Stop();
        StopCoroutine(nameof(PlayLoops));
    }

    private void OnDestroy()
    {
        if (!eventRegistered) return;
        
        CalibrationRunManager.OnPointReached -= OnReachNode;
        CalibrationRunManager.OnCalibrationCompleted -= StopMusic;
        CalibrationRunManager.OnCalibrationFailed -= StopMusic;
        ProtoDestructionEvent.OnSubDestroyed -= OnSubDestroyed;
    }

    private void OnSubDestroyed()
    {
        stopSfx = true;
        currentSfxLoop.Stop();
        StopCoroutine(nameof(PlayLoops));
    }
}