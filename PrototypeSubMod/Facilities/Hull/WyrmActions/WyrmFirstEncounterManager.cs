using System;
using PrototypeSubMod.PrototypeStory.CalibrationSite;
using Story;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmFirstEncounterManager : MonoBehaviour
{
    [SerializeField] private ProtoAggressiveWorm aggressiveWorm;
    [SerializeField] private WyrmAction[] predeterminedActions;
    [SerializeField] private float firstAggressionTime;

    private bool startedSequence;
    private int actionStage;
    
    private void Start()
    {
        CalibrationRunManager.OnCalibrationCompleted += OnCalibrationCompleted;
        
        if (FirstEncounterCompleted()) return;

        aggressiveWorm.SetTimeInVoidForAggression(firstAggressionTime);
    }

    private void Update()
    {
        if (FirstEncounterCompleted()) return;
        
        if (startedSequence && !aggressiveWorm.IsAggressive())
        {
            actionStage = 0;
            startedSequence = false;
        }
        
        if (startedSequence || !aggressiveWorm.IsAggressive()) return;

        foreach (var wyrmAction in aggressiveWorm.actions)
        {
            wyrmAction.SendMessage("OverrideStopPerform", SendMessageOptions.DontRequireReceiver);
        }
        
        var action = predeterminedActions[actionStage];
        action.Perform(null, 0, 0);
        action.OnActionComplete += OnActionCompleted;
        startedSequence = true;
    }

    private void OnActionCompleted()
    {
        predeterminedActions[actionStage].OnActionComplete -= OnActionCompleted;
        actionStage++;

        if (actionStage >= predeterminedActions.Length)
        {
            return;
        }
        
        var newAction = predeterminedActions[actionStage];
        newAction.Perform(null, 0, 0);
        newAction.OnActionComplete += OnActionCompleted;
    }

    private bool FirstEncounterCompleted()
    {
        return StoryGoalManager.main.IsGoalComplete("WyrmFirstEncounterComplete");
    }

    public bool IsManagingActions()
    {
        return actionStage < predeterminedActions.Length;
    }

    private void OnCalibrationCompleted()
    {
        StoryGoalManager.main.OnGoalComplete("WyrmFirstEncounterComplete");
        aggressiveWorm.ForceDespawn();
    }

    private void OnDestroy()
    {
        CalibrationRunManager.OnCalibrationCompleted -= OnCalibrationCompleted;
    }
}