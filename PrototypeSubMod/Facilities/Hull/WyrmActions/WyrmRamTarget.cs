using System.Collections.Generic;
using PrototypeSubMod.LightDistortionField;
using PrototypeSubMod.Patches;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmRamTarget : WyrmAction
{
    [SerializeField] private float attackDamage = 200;
    [SerializeField] private float attackRadius;
    [SerializeField] private float impulseForce;
    
    [Header("SFX")]
    [SerializeField] private WyrmRoarManager roarManager;
    [SerializeField] private FMOD_CustomEmitter chargeImpactSfx;
    
    private bool hasDamagedTarget;

    private void Start()
    {
        onReachedTarget += OnReachedPoint;
    }
    
    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;
        
        base.Perform(creature, time, deltaTime);
        hasDamagedTarget = false;
        
        Plugin.Logger.LogInfo($"Started ram target");
    }
    
    private void Update()
    {
        if (!performing) return;
        
        wormAnimator.SetTravelTarget(GetMovementPoints()[AttackStage], OnReachedTarget);
    }
    
    protected override Vector3[] GetMovementPoints()
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
        else if (player.currentSub != null)
        {
            points[1] = targetCenter + forwardDir * 10f;
        }
        else
        {
            points[1] = targetCenter;
        }

        return points;
    }

    private void OnReachedPoint()
    {
        var pointsLength = GetMovementPoints().Length;
        if (AttackStage == 1)
        {
            roarManager.PlayRoar(Player.main.transform.position);
        }
        
        if (AttackStage <= pointsLength - 1 || hasDamagedTarget) return;
        
        var colliders = Physics.OverlapSphere(transform.position, attackRadius);
        foreach (var col in colliders)
        {
            var subRoot = col.GetComponentInParent<SubRoot>();

            if (!subRoot) continue;
            if (subRoot.GetComponentInChildren<CloakEffectHandler>().GetActive()) continue;

            subRoot.live.TakeDamage(attackDamage, transform.position, DamageType.Drill, gameObject);
            hasDamagedTarget = true;
            chargeImpactSfx.Play();
            MainCameraControl.main.ShakeCamera(5);

            subRoot.rigidbody.AddForce(transform.forward * impulseForce, ForceMode.Impulse);
            break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}