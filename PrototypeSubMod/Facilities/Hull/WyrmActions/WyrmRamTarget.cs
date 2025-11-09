using System.Collections.Generic;
using PrototypeSubMod.LightDistortionField;
using PrototypeSubMod.Patches;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmRamTarget : CreatureAction
{
    [SerializeField] private AggressiveWormAnimator wormAnimator;
    [SerializeField] private float attackDamage = 200;
    [SerializeField] private float attackRadius;

    private bool performing;
    private bool hasDamagedTarget;
    private int attackStage;
    
    public override float Evaluate(Creature creature, float time)
    {
        return performing ? 1 : Random.Range(0f, 0.8f);
    }

    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;
        
        base.Perform(creature, time, deltaTime);
        performing = true;
        hasDamagedTarget = false;
        attackStage = 0;
        wormAnimator.SetTravelTarget(GetAttackPoints()[attackStage], OnReachedTarget);
        Plugin.Logger.LogInfo($"Started ram target");
    }
    
    public void OverrideStopPerform()
    {
        performing = false;
    }


    private void Update()
    {
        if (!performing) return;
        
        wormAnimator.SetTravelTarget(GetAttackPoints()[attackStage], OnReachedTarget);
    }
    
    private Vector3[] GetAttackPoints()
    {
        const float setupDist = 150;
        
        var points = new Vector3[2];
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

        var effectHandler = player.currentSub?.GetComponentInChildren<CloakEffectHandler>();
        
        var forwardDir = targetCenter.normalized;
        var rightDir = -Vector3.Cross(forwardDir, Vector3.up);
        // Offset to the right to set up for the swing towards the target
        points[0] = targetCenter + (forwardDir + rightDir) * setupDist - Vector3.up * 2f;
        // Go for the target
        if (effectHandler && effectHandler.GetActive())
        {
            points[1] = effectHandler.GetContinuousPointOnSurface(15f);
        }
        else
        {
            points[1] = targetCenter + forwardDir * 10f;
        }

        return points;
    }

    private void OnReachedTarget()
    {
        attackStage++;
        
        if (attackStage <= GetAttackPoints().Length - 1) return;
        
        performing = false;
        if (hasDamagedTarget) return;
        
        var colliders = Physics.OverlapSphere(transform.position, attackRadius);
        foreach (var col in colliders)
        {
            var subRoot = col.GetComponentInParent<SubRoot>();
            if (!subRoot) continue;

            if (subRoot.GetComponentInChildren<CloakEffectHandler>().GetActive()) continue;
            
            subRoot.live.TakeDamage(attackDamage, transform.position, DamageType.Drill, gameObject);
            Plugin.Logger.LogInfo($"Damaging {subRoot} for {attackDamage}");
            hasDamagedTarget = true;
            break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
    
    public override bool NeedsToBeChecked(float time) => true;
}