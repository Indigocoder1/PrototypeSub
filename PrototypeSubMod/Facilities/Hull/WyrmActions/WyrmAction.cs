using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public abstract class WyrmAction : CreatureAction
{
    [SerializeField] protected AggressiveWormAnimator wormAnimator;
    [Tooltip("From 0-1, with 1 being most like and 0 being least likely")]
    [SerializeField] private float activationChance;

    public event Action OnActionComplete;
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
        
        base.Perform(creature, time, deltaTime);

        wormAnimator.SetTravelTarget(GetMovementPoints()[AttackStage], OnReachedTargetPoint);
        aggressiveWorm.OnActionStarted(this);
    }

    public void OverrideStopPerform()
    {
        performing = false;
    }

    protected void OnReachedTargetPoint()
    {
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
    }

    protected abstract Vector3[] GetMovementPoints();
    
    public override bool NeedsToBeChecked(float time) => true;
}