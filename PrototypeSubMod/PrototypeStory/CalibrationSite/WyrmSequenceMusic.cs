using System;
using System.Collections;
using PrototypeSubMod.Facilities.Hull.WyrmActions;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class WyrmSequenceMusic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WyrmFirstEncounterManager encounterManager;
    [SerializeField] private WyrmAction dartAction;
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
    
    private void Start()
    {
        if (encounterManager.FirstEncounterCompleted()) return;
        
        currentSfxLoop = introOneShot;
        nextLoopWanted = dartActionLoop;
        StartCoroutine(PlayLoops());
        
        ramAction.OnActionComplete += OnRamHitTarget;
        laserAction.OnStartTargeting += OnLaserTargetingStart;
        laserAction.OnLaserImpact += OnLaserImpact;
        CalibrationRunManager.OnPointReached += OnReachNode;
        eventRegistered = true;
    }

    private IEnumerator PlayLoops()
    {
        currentSfxLoop.Play();
        
        while (gameObject.activeInHierarchy)
        {
            yield return null;
            
            if (currentSfxLoop.playing) continue;

            if (nextLoopWanted != null)
            {
                currentSfxLoop = nextLoopWanted;
                Plugin.Logger.LogInfo($"Assigning current SFX to {nextLoopWanted}");
                nextLoopWanted = null;
            }
            
            currentSfxLoop.Play();
            Plugin.Logger.LogInfo($"Playing current SFX");
        }

        currentSfxLoop.Stop();
    }

    private void OnRamHitTarget()
    {
        nextLoopWanted = laserActionLoop;
        ramAction.OnActionComplete -= OnRamHitTarget;
    }

    private void OnLaserTargetingStart()
    {
        StopCoroutine(nameof(PlayLoops));
        currentSfxLoop.Stop();
        
        laserAction.OnStartTargeting -= OnLaserTargetingStart;
    }

    private void OnLaserImpact()
    {
        currentSfxLoop = intenseIntro;
        nextLoopWanted = intense1;
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
    }
}