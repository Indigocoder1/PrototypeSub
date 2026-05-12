using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmFollowPlayer : WyrmAction
{
    [SerializeField] private float offsetFromPlayer;
    [SerializeField] private float timeBetweenPointRecalculations = 15f;
    [Range(0, 90)]
    [SerializeField] private float maxAngleFromForward;
    
    private float timeLastPerformed;

    public override float Evaluate(Creature creature, float time)
    {
        return !aggressiveWorm.IsAggressive() ? 1f : base.Evaluate(creature, time);
    }

    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;

        if (Time.time < timeLastPerformed + timeBetweenPointRecalculations) return;

        base.Perform(creature, time, deltaTime);
        
        Plugin.Logger.LogInfo($"Starting wyrm follow player");
        timeLastPerformed = Time.time;
    }

    protected override Vector3[] GetMovementPoints()
    {
        var dir = Random.onUnitSphere;
        var forward = Player.main.transform.position.normalized;
        dir *= Mathf.Sign(Vector3.Dot(dir, forward));
        float angleBetween = Vector3.Angle(dir, forward);
        dir = Vector3.RotateTowards(dir, forward, angleBetween * (1 - maxAngleFromForward / 90) * Mathf.Deg2Rad, 1);

        var currentSub = Player.main.currentSub;
        if (currentSub && Vector3.Dot(transform.forward, currentSub.transform.forward) < 0)
        {
            var sign = Mathf.Sign(Random.Range(-2f, 2f));
            sign = sign == 0 ? 1 : sign;
            dir = currentSub.transform.right * sign;
        }
        
        var targetPoint = Player.main.transform.position + dir.normalized * offsetFromPlayer;
        return new[] { targetPoint };
    }
}