using System;
using FMOD.Studio;
using PrototypeSubMod.PrototypeStory.CalibrationSite;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class FireMusicManager : MonoBehaviour
{
    [SerializeField] private FMOD_CustomEmitter fireMusic;

    private bool inCalibrationRun;
    
    private void Start()
    {
        CalibrationRunManager.OnCalibrationStarted += OnCalibrationStarted;
        CalibrationRunManager.OnCalibrationCompleted += OnCalibrationStopped;
        CalibrationRunManager.OnCalibrationFailed += OnCalibrationStopped;
    }

    private void OnCalibrationStarted() => inCalibrationRun = true;
    private void OnCalibrationStopped() => inCalibrationRun = false;

    public void NewAlarmState()
    {
        if (!inCalibrationRun) return;

        fireMusic.Stop(STOP_MODE.IMMEDIATE);
    }

    private void OnDestroy()
    {
        CalibrationRunManager.OnCalibrationStarted -= OnCalibrationStarted;
        CalibrationRunManager.OnCalibrationCompleted -= OnCalibrationStopped;
        CalibrationRunManager.OnCalibrationFailed -= OnCalibrationStopped;
    }
}