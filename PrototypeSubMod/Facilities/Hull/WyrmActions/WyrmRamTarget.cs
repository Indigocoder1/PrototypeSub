using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmRamTarget : CreatureAction
{
    [SerializeField] private AggressiveWormAnimator wormAnimator;
    [SerializeField] private float playerDamage = 50;
    [SerializeField] private float submarineDamage = 200;
    [SerializeField] private float attackRadius;

    private bool performing;
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
        attackStage = 0;
        wormAnimator.SetTravelTarget(GetSetupPoints()[attackStage], OnReachedTarget);
        Plugin.Logger.LogInfo($"Started ram target");
    }

    private void Update()
    {
        if (!performing) return;
        
        wormAnimator.SetTravelTarget(GetSetupPoints()[attackStage], OnReachedTarget);
    }
    
    private Vector3[] GetSetupPoints()
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
        // Offset to the right to setup for the swing towards the player
        points[0] = targetCenter + rightDir * setupDist;
        // Lower the target point slightly to make the wyrm loop vertically for the final attack
        points[1] = targetCenter + forwardDir * setupDist - Vector3.up * 2f;
        // Go for the player
        points[2] = targetCenter;

        return points;
    }

    private void OnReachedTarget()
    {
        attackStage++;
        if (attackStage > GetSetupPoints().Length - 1)
        {
            performing = false;
            var colliders = Physics.OverlapSphere(transform.position, attackRadius);
            List<LiveMixin> damagedMixins = new();
            foreach (var col in colliders)
            {
                var mixin = col.GetComponentInParent<LiveMixin>();
                if (!mixin || damagedMixins.Contains(mixin)) continue;

                float damage = mixin.gameObject.TryGetComponent(out SubRoot _) ? submarineDamage : playerDamage;
                mixin.TakeDamage(damage, type: DamageType.Drill);
                damagedMixins.Add(mixin);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}