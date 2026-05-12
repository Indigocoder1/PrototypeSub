using System;
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
    [SerializeField] private FMOD_CustomLoopingEmitter dartActionLoop;
    [SerializeField] private FMOD_CustomLoopingEmitter laserActionLoop;
    [SerializeField] private FMOD_CustomLoopingEmitter laserImpact;
    [SerializeField] private FMOD_CustomLoopingEmitter node2Loop;
    [SerializeField] private FMOD_CustomLoopingEmitter node4Loop;
    [SerializeField] private FMOD_CustomEmitter musicOutro;

    private bool eventRegistered;
    
    private void Start()
    {
        if (encounterManager.FirstEncounterCompleted()) return;

        introOneShot.Play();
        dartAction.OnActionStart += OnDartStart;
        ramAction.OnActionComplete += OnRamHitTarget;
        laserAction.OnStartTargeting += OnLaserTargetingStart;
        laserAction.OnLaserImpact += OnLaserImpact;
        CalibrationRunManager.OnPointReached += OnReachNode;
        eventRegistered = true;
    }

    private void OnDartStart()
    {
        introOneShot.Stop();
        dartActionLoop.Play();
        dartAction.OnActionStart -= OnDartStart;
    }

    private void OnRamHitTarget()
    {
        dartActionLoop.Stop();
        laserActionLoop.Start();
        ramAction.OnActionComplete -= OnRamHitTarget;
    }

    private void OnLaserTargetingStart()
    {
        laserActionLoop.Stop();
        laserAction.OnStartTargeting -= OnLaserTargetingStart;
    }

    private void OnLaserImpact()
    {
        laserImpact.Play();
        laserAction.OnLaserImpact -= OnLaserImpact;
    }
    
    private void OnReachNode(int point)
    {
        switch (point)
        {
            case 1:
                laserImpact.Stop();
                node2Loop.Play();
                break;
            case 3:
                node2Loop.Stop();
                node4Loop.Play();
                break;
            case 5:
                node4Loop.Stop();
                musicOutro.Play();
                break;
        }
    }

    private void OnDestroy()
    {
        if (!eventRegistered) return;
        
        CalibrationRunManager.OnPointReached -= OnReachNode;
    }
}