using System;
using System.Collections;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationCompletionManager : MonoBehaviour
{
    [SerializeField] private CyclopsMotorMode motorMode;
    [SerializeField] private EmissionColorController emissionController;
    [SerializeField] private float disableDuration;
    
    [Header("SFX")]
    [SerializeField] private VoiceNotificationManager voiceNotificationManager;
    [SerializeField] private VoiceNotification engineRestartNotification;
    [SerializeField] private FMOD_CustomEmitter powerDownSfx;
    [SerializeField] private FMOD_CustomEmitter powerUpSfx;

    private void Start()
    {
        CalibrationRunManager.OnCalibrationCompleted += OnCalibrationCompleted;
    }

    private void OnCalibrationCompleted()
    {
        StartCoroutine(DisableEngineAsync());
    }

    private IEnumerator DisableEngineAsync()
    {
        var engineWasOn = motorMode.engineOn;
        motorMode.engineOn = false;
        motorMode.subController.NewEngineMode(false);
        emissionController.RegisterTempColor(this, new EmissionColorController.EmissionRegistrarData(Color.black, 20));
        powerDownSfx.Play();

        yield return new WaitForSeconds(disableDuration);

        motorMode.engineOn = engineWasOn;
        motorMode.subController.NewEngineMode(engineWasOn);
        emissionController.RemoveTempColor(this);
        voiceNotificationManager.PlayVoiceNotification(engineRestartNotification);
        powerUpSfx.Play();
    }

    private void OnDestroy()
    {
        CalibrationRunManager.OnCalibrationCompleted -= OnCalibrationCompleted;
    }
}