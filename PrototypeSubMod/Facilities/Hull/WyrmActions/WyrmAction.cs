using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public abstract class WyrmAction : CreatureAction
{
    [SerializeField] protected AggressiveWormAnimator wormAnimator;
    [Tooltip("From 0-1, with 1 being most like and 0 being least likely")]
    [SerializeField] private float activationChance;
    
    protected int AttackStage { get; private set; }
    protected event Action onReachedTarget;
    protected bool performing;
    protected ProtoAggressiveWorm aggressiveWorm;

    private new void Awake()
    {
        aggressiveWorm = GetComponent<ProtoAggressiveWorm>();
    }

    public override float Evaluate(Creature creature, float time)
    {
        if (aggressiveWorm.WasActionRecentlyStarted(this) && !performing) return 0;

        if (performing) return 1;

        return Random.Range(0, activationChance);
    }

    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;

        performing = true;
        AttackStage = 0;
        
        base.Perform(creature, time, deltaTime);

        wormAnimator.SetTravelTarget(GetMovementPoints()[AttackStage], OnReachedTarget);
        aggressiveWorm.OnActionStarted(this);
    }

    public void OverrideStopPerform()
    {
        performing = false;
    }

    protected void OnReachedTarget()
    {
        AttackStage++;
        
        onReachedTarget?.Invoke();

        var movementPoints = GetMovementPoints();
        if (AttackStage >= movementPoints.Length)
        {
            performing = false;
            return;
        }
        
        wormAnimator.SetTravelTarget(GetMovementPoints()[AttackStage], OnReachedTarget);
    }

    protected abstract Vector3[] GetMovementPoints();
    
    public override bool NeedsToBeChecked(float time) => true;
}