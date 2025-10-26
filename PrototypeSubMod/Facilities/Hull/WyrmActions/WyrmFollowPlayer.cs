using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmFollowPlayer : CreatureAction
{
    [SerializeField] private AggressiveWormAnimator wormAnimator;
    [SerializeField] private float offsetFromPlayer;
    [SerializeField] private float timeBetweenPointRecalculations = 15f;
    [Range(0, 90)]
    [SerializeField] private float maxAngleFromForward;
    
    private Vector3 targetPoint;
    private bool performing;
    private float timeLastPerformed;

    private void Start()
    {
        RecalculateTargetPoint();
    }

    public override float Evaluate(Creature creature, float time)
    {
        bool aggressive = ((ProtoAggressiveWorm)creature).IsAggressive();
        Plugin.Logger.LogInfo($"Evaluating FollowTarget | Aggressive = {aggressive} | Performing = {performing}");
        if (!aggressive)
        {
            return 1;
        }
        
        return performing ? 1 : Random.Range(0f, 0.8f);
    }

    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;
        
        base.Perform(creature, time, deltaTime);

        if (Time.time < timeLastPerformed + timeBetweenPointRecalculations) return;
        
        RecalculateTargetPoint();
        timeLastPerformed = Time.time;
        performing = true;
    }

    private void RecalculateTargetPoint()
    {
        var dir = Random.onUnitSphere;
        var forward = Player.main.transform.position.normalized;
        dir *= Mathf.Sign(Vector3.Dot(dir, forward));
        float angleBetween = Vector3.Angle(dir, forward);
        dir = Vector3.RotateTowards(dir, forward, angleBetween * (1 - maxAngleFromForward / 90) * Mathf.Deg2Rad, 1);
        
        targetPoint = Player.main.transform.position + dir.normalized * offsetFromPlayer;
        wormAnimator.SetTravelTarget(targetPoint, OnReachTarget);
        Plugin.Logger.LogInfo($"Recalculating target point on {gameObject}");
    }

    private void OnReachTarget()
    {
        performing = false;
    }
    
    public override bool NeedsToBeChecked(float time) => true;
}