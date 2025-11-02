using System;
using PrototypeSubMod.LightDistortionField;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmShootTarget : CreatureAction
{
    [SerializeField] private AggressiveWormAnimator wormAnimator;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private float attackDamage;
    [SerializeField] private float chargeUpTime;
    [SerializeField] private int parriesToResetAggression = 3;
    [SerializeField] private float timePassiveAfterParries;
    
    private bool performing;
    private bool canShoot;
    private bool hasShot;
    private float currentChargeUpTime;
    private int prevChargeUpTime;
    private int rightHandVectorSign;
    private int attackStage;
    private int timesParried;

    private void Start()
    {
        lineRenderer.enabled = false;
    }

    public override float Evaluate(Creature creature, float time)
    {
        return performing ? 1 : Random.Range(0f, 0.8f);
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

    public void OnShotParried()
    {
        ErrorMessage.AddError("Parried!");
        timesParried++;

        if (timesParried >= parriesToResetAggression)
        {
            ErrorMessage.AddError($"Resetting aggression for {timePassiveAfterParries} seconds");
            GetComponent<ProtoAggressiveWorm>().ResetAggression(timePassiveAfterParries);
        }
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
        var targetMixin = GetTargetMixin();
        var effectHandler = targetMixin.GetComponentInChildren<CloakEffectHandler>();
        if (effectHandler && effectHandler.GetActive())
        {
            ErrorMessage.AddError("Missed!");
        }
        else
        {
            targetMixin.TakeDamage(attackDamage, transform.position, DamageType.LaserCutter, gameObject);
        }
            
        ErrorMessage.AddError("Pew");
    }

    private void HandleLaser()
    {
        var targetMixin = GetTargetMixin();
        var targetPos = targetMixin.transform.position;
        var dirToTarget = (targetPos - laserOrigin.position).normalized;
        const float targetOffset = 1f;
        var positions = new Vector3[2];
        positions[0] = laserOrigin.position;
        var effectHandler = targetMixin.GetComponentInChildren<CloakEffectHandler>();
        if (effectHandler && effectHandler.GetActive())
        {
            positions[1] = effectHandler.GetContinuousPointOnSurface(targetOffset);
        }
        else
        {
            positions[1] = targetPos - dirToTarget * targetOffset;
        }
        lineRenderer.SetPositions(positions);
    }
    
    private Vector3[] GetAttackPoints()
    {
        const float setupDist = 200;
        
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
        points[0] = targetCenter + (forwardDir + rightDir) * setupDist;
        // Go off towards the right
        points[1] = targetCenter + (forwardDir + rightDir * rightHandVectorSign) * setupDist;
        // Straight towards target
        points[2] = targetCenter - Vector3.up * 30f;

        return points;
    }

    private LiveMixin GetTargetMixin()
    {
        var player = Player.main;
        if (player.currentSub) return player.currentSub.live;
        if (player.lastValidSub &&
            Vector3.Distance(player.lastValidSub.transform.position, player.transform.position) < 50f)
        {
            return player.lastValidSub.live;
        }

        return player.liveMixin;
    }

    public override bool NeedsToBeChecked(float time) => true;
}