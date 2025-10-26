using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmShootTarget : CreatureAction
{
    [SerializeField] private AggressiveWormAnimator wormAnimator;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private float attackDamage;
    [SerializeField] private float chargeUpTime;
    
    private bool performing;
    private bool canShoot;
    private bool hasShot;
    private float currentChargeUpTime;
    private int prevChargeUpTime;
    private int rightHandVectorSign;
    private int attackStage;
    
    public override float Evaluate(Creature creature, float time)
    {
        return performing ? 1 : Random.Range(0.4f, 0.6f);
    }
    
    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;
        
        base.Perform(creature, time, deltaTime);
        performing = true;
        canShoot = false;
        hasShot = false;
        lineRenderer.enabled = false;
        rightHandVectorSign = (int)Mathf.Sign(Random.Range(-1f, 1f));
        attackStage = 0;
        currentChargeUpTime = 0;
        wormAnimator.SetTravelTarget(GetAttackPoints()[attackStage], OnReachedTarget);
        Plugin.Logger.LogInfo($"Started shoot target");
    }
    
    private void Update()
    {
        if (!performing) return;
        
        wormAnimator.SetTravelTarget(GetAttackPoints()[attackStage], OnReachedTarget);
        if (currentChargeUpTime > 0)
        {
            currentChargeUpTime -= Time.deltaTime;
            HandleLaser();
        }
        else if (canShoot && !hasShot)
        {
            Shoot();
        }

        var angle = Mathf.Abs(
            Vector3.Angle(GetTargetMixin().transform.position - transform.position, transform.forward));
        const float angleToChargeLaser = 30f;
        if (attackStage == 2 && angle < angleToChargeLaser && !canShoot && !hasShot)
        {
            currentChargeUpTime = chargeUpTime;
            canShoot = true;
            lineRenderer.enabled = true;
        }

        if (prevChargeUpTime != (int)currentChargeUpTime)
        {
            ErrorMessage.AddError($"Shooting in {(int)currentChargeUpTime + 1}");
        }

        prevChargeUpTime = (int)currentChargeUpTime;
    }

    private void OnReachedTarget()
    {
        attackStage++;
        if (attackStage > GetAttackPoints().Length - 1)
        {
            performing = false;
        }
    }

    private void Shoot()
    {
        canShoot = false;
        hasShot = true;
        lineRenderer.enabled = false;
        GetTargetMixin().TakeDamage(attackDamage, type: DamageType.LaserCutter);
        ErrorMessage.AddError("Pew");
    }

    private void HandleLaser()
    {
        var targetPos = GetTargetMixin().transform.position;
        var dirToTarget = (targetPos - laserOrigin.position).normalized;
        var positions = new Vector3[2];
        positions[0] = laserOrigin.position;
        positions[1] = targetPos - dirToTarget;
        lineRenderer.SetPositions(positions);
    }
    
    private Vector3[] GetAttackPoints()
    {
        const float setupDist = 100;
        
        var points = new Vector3[3];
        var player = Player.main;
        Vector3 targetCenter;
        if (player.currentSub == null)
        {
            targetCenter = player.transform.position;
        }
        else
        {
            targetCenter = player.currentSub.centerOfMass.position;
        }
        
        var forwardDir = targetCenter.normalized;
        var rightDir = -Vector3.Cross(forwardDir, Vector3.up);
        // Offset to the right to set up for the swing towards the target
        points[0] = targetCenter + rightDir * setupDist;
        // Go off towards the right
        points[1] = targetCenter + (forwardDir + rightDir * rightHandVectorSign) * setupDist;
        // Straight towards target
        points[2] = targetCenter;

        return points;
    }

    private LiveMixin GetTargetMixin()
    {
        var player = Player.main;
        return player.currentSub ? player.currentSub.live : player.liveMixin;
    }
}