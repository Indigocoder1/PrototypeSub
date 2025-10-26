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
    private float timeLastPerformed;

    private void Start()
    {
        RecalculateTargetPoint();
    }

    public override float Evaluate(Creature creature, float time)
    {
        return ((ProtoAggressiveWorm)creature).IsAggressive() ? 0 : 1;
    }

    public override void Perform(Creature creature, float time, float deltaTime)
    {
        base.Perform(creature, time, deltaTime);

        if (Time.time < timeLastPerformed + timeBetweenPointRecalculations) return;
        
        RecalculateTargetPoint();
        timeLastPerformed = Time.time;
    }

    private void RecalculateTargetPoint()
    {
        var dir = Random.onUnitSphere;
        dir *= Mathf.Sign(Vector3.Dot(dir, dir));
        float angleBetween = Vector3.Angle(dir, dir);
        dir = Vector3.RotateTowards(dir, dir, angleBetween * (1 - maxAngleFromForward / 90) * Mathf.Deg2Rad, 1);
        
        targetPoint = Player.main.transform.position + dir.normalized * offsetFromPlayer;
        wormAnimator.SetTravelTarget(targetPoint, RecalculateTargetPoint);
        Plugin.Logger.LogInfo($"Recalculating target point on {gameObject}");
    }
}