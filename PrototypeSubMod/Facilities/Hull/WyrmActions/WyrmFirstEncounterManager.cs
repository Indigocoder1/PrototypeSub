using System;
using PrototypeSubMod.PrototypeStory.CalibrationSite;
using Story;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmFirstEncounterManager : MonoBehaviour
{
    public static event Action OnFirstEncounterStarted;
    public static event Action OnFirstEncounterEnded;
    public static event Action OnDespawned;
    
    [SerializeField] private ProtoAggressiveWorm aggressiveWorm;
    [SerializeField] private WyrmAction[] predeterminedActions;
    [SerializeField] private float firstAggressionTime;

    private bool doingCalibrationRun;
    private bool startedSequence;
    private int actionStage;
    
    private void Start()
    {
        CalibrationRunManager.OnCalibrationCompleted += OnCalibrationCompleted;
        CalibrationRunManager.OnCalibrationFailed += OnCalibrationFailed;
        CalibrationRunManager.OnPointReached += OnPointReached;
        aggressiveWorm.OnDespawn += OnDespawned;

        foreach (var wyrmAction in aggressiveWorm.actions)
        {
            wyrmAction.SendMessage("OverrideStopPerform", SendMessageOptions.DontRequireReceiver);
        }
        
        var runManager = FindObjectOfType<CalibrationRunManager>();
        if (runManager && runManager.IsDoingCalibrationRun())
        {
            doingCalibrationRun = true;
        }
        
        if (FirstEncounterCompleted()) return;

        actionStage = 0;
        aggressiveWorm.SetTimeInVoidForAggression(firstAggressionTime);
        OnFirstEncounterStarted?.Invoke();
    }

    private void Update()
    {
        if (FirstEncounterCompleted()) return;
        
        if (startedSequence || !aggressiveWorm.IsAggressive()) return;
        
        foreach (var wyrmAction in aggressiveWorm.actions)
        {
            wyrmAction.SendMessage("OverrideStopPerform", SendMessageOptions.DontRequireReceiver);
        }
        
        var action = predeterminedActions[actionStage];
        Plugin.Logger.LogInfo($"Starting {action} from Update");
        action.Perform(null, 0, 0);
        action.OnActionComplete += OnActionCompleted;
        startedSequence = true;
    }

    private void OnActionCompleted()
    {
        Plugin.Logger.LogInfo($"{predeterminedActions[actionStage]} completed");
        predeterminedActions[actionStage].ClearActionCompleteListeners();

        // Keep doing the dart action until reaching the calibration start
        if (doingCalibrationRun)
        {
            actionStage++;
        }

        if (actionStage >= predeterminedActions.Length)
        {
            return;
        }
        
        var newAction = predeterminedActions[actionStage];
        Plugin.Logger.LogInfo($"Starting {newAction} from OnActionCompleted");
        newAction.Perform(null, 0, 0);
        newAction.OnActionComplete += OnActionCompleted;
    }

    public bool FirstEncounterCompleted()
    {
        return StoryGoalManager.main.IsGoalComplete("WyrmFirstEncounterComplete");
    }

    public bool IsManagingActions()
    {
        return actionStage < predeterminedActions.Length && !FirstEncounterCompleted();
    }

    private void OnCalibrationCompleted()
    {
        StoryGoalManager.main.OnGoalComplete("WyrmFirstEncounterComplete");
        aggressiveWorm.ForceDespawn();
        OnFirstEncounterEnded?.Invoke();
        doingCalibrationRun = false;
    }

    private void OnPointReached(int index)
    {
        if (index != 0) return;
        
        actionStage = 0;
        startedSequence = false;
        doingCalibrationRun = true;
    }

    private void OnCalibrationFailed()
    {
        doingCalibrationRun = false;
    }

    private void OnDestroy()
    {
        CalibrationRunManager.OnCalibrationCompleted -= OnCalibrationCompleted;
        CalibrationRunManager.OnCalibrationFailed -= OnCalibrationFailed;
        CalibrationRunManager.OnPointReached -= OnPointReached;
    }
}