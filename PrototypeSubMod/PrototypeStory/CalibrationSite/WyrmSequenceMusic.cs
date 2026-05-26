using System;
using System.Collections;
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
    [SerializeField] private FMOD_CustomEmitter musicOutro;

    private FMOD_CustomEmitter currentSfxLoop;
    private FMOD_CustomEmitter nextLoopWanted;
    private bool eventRegistered;
    private bool stopSfx;
    
    private void Start()
    {
        if (encounterManager.FirstEncounterCompleted()) return;
        
        ramAction.OnActionComplete += OnRamHitTarget;
        laserAction.OnStartTargeting += OnLaserTargetingStart;
        laserAction.OnLaserImpact += OnLaserImpact;
        CalibrationRunManager.OnPointReached += OnReachNode;
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

        if (nextLoopWanted != null)
        {
            currentSfxLoop = nextLoopWanted;
            nextLoopWanted = null;
        }

        if (!stopSfx)
        {
            currentSfxLoop.Play();
            StartCoroutine(PlayLoops());
        }
        
        stopSfx = false;
    }

    private void OnRamHitTarget()
    {
        nextLoopWanted = laserActionLoop;
        ramAction.OnActionComplete -= OnRamHitTarget;
    }

    private void OnLaserTargetingStart()
    {
        stopSfx = true;
        currentSfxLoop.Stop();
        StopCoroutine(nameof(PlayLoops));
        
        laserAction.OnStartTargeting -= OnLaserTargetingStart;
    }

    private void OnLaserImpact()
    {
        currentSfxLoop = intenseIntro;
        nextLoopWanted = intense1;
        stopSfx = false;
        StartCoroutine(PlayLoops());
        laserAction.OnLaserImpact -= OnLaserImpact;
    }
    
    private void OnReachNode(int point)
    {
        nextLoopWanted = point switch
        {
            2 => intense2,
            4 => intense3,
            5 => musicOutro,
            _ => nextLoopWanted
        };
    }

    private void OnDestroy()
    {
        if (!eventRegistered) return;
        
        CalibrationRunManager.OnPointReached -= OnReachNode;
        ProtoDestructionEvent.OnSubDestroyed -= OnSubDestroyed;
    }

    private void OnSubDestroyed()
    {
        stopSfx = true;
        currentSfxLoop.Stop();
        StopCoroutine(nameof(PlayLoops));
    }
}