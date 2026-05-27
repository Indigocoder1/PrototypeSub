using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public abstract class WyrmAction : CreatureAction
{
    [SerializeField] protected AggressiveWormAnimator wormAnimator;
    [Tooltip("From 0-1, with 1 being most like and 0 being least likely")]
    [SerializeField] private float activationChance;
    [SerializeField] private float maxTimePerStage = 20f;

    public event Action OnActionComplete;
    public event Action OnActionStart;
    protected int AttackStage { get; private set; }
    protected event Action OnReachedTarget;
    protected bool performing;
    protected ProtoAggressiveWorm aggressiveWorm;
    private WyrmFirstEncounterManager firstEncounterManager;

    private new void Awake()
    {
        aggressiveWorm = GetComponent<ProtoAggressiveWorm>();
        firstEncounterManager = GetComponent<WyrmFirstEncounterManager>();
    }

    public override float Evaluate(Creature creature, float time)
    {
        if (aggressiveWorm.IsDespawning()) return 0;
        
        if (performing) return 1;
        
        if (aggressiveWorm.WasActionRecentlyStarted(this) && !performing) return 0;

        if (firstEncounterManager && firstEncounterManager.IsManagingActions()) return 0;
        
        return Random.Range(0, activationChance);
    }

    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;

        performing = true;
        AttackStage = 0;
        OnActionStart?.Invoke();
        
        base.Perform(creature, time, deltaTime);

        wormAnimator.SetTravelTarget(GetMovementPoints()[AttackStage], OnReachedTargetPoint);
        aggressiveWorm.OnActionStarted(this);
        StartCoroutine(ProgressActionDelayed());
    }

    public virtual void OverrideStopPerform()
    {
        performing = false;
        // Clear all listeners
        OnActionComplete = null;
    }

    protected void OnReachedTargetPoint()
    {
        if (!performing) return;
        
        AttackStage++;
        
        OnReachedTarget?.Invoke();

        var movementPoints = GetMovementPoints();
        if (AttackStage >= movementPoints.Length)
        {
            performing = false;
            OnActionComplete?.Invoke();
            return;
        }
        
        wormAnimator.SetTravelTarget(GetMovementPoints()[AttackStage], OnReachedTargetPoint);
        StartCoroutine(ProgressActionDelayed());
    }

    protected void ForceActionComplete()
    {
        performing = false;
        OnActionComplete?.Invoke();
    }

    public void ClearActionCompleteListeners()
    {
        OnActionComplete = null;
    }

    public bool IsPerforming() => performing;

    private IEnumerator ProgressActionDelayed()
    {
        int stageWhenStarted = AttackStage;
        yield return new WaitForSeconds(maxTimePerStage);
        if (!performing) yield break;
        if (AttackStage != stageWhenStarted) yield break;
        if (AttackStage == GetMovementPoints().Length - 1) yield break;

        Plugin.Logger.LogInfo($"Spent too much time on stage {AttackStage} for {this}. Force progressing stage");
        OnReachedTargetPoint();
    }

    protected abstract Vector3[] GetMovementPoints();
    
    public override bool NeedsToBeChecked(float time) => true;
}