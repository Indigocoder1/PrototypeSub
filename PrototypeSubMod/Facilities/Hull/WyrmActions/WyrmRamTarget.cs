using PrototypeSubMod.LightDistortionField;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmRamTarget : WyrmAction
{
    [SerializeField] private WyrmShoveSub shoveSub;
    [SerializeField] private float attackDamage = 200;
    
    [Header("SFX")]
    [SerializeField] private WyrmRoarManager roarManager;
    
    private bool hasDamagedTarget;

    private void Start()
    {
        OnReachedTarget += OnReachedPoint;
        shoveSub.OnHitSub += OnHitSub;
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
        
        wormAnimator.SetTravelTarget(GetMovementPoints()[AttackStage], OnReachedTargetPoint);
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
        if (AttackStage == 1)
        {
            roarManager.PlayRoar(Player.main.transform.position);
        }
    }

    private void OnHitSub(SubRoot subRoot)
    {
        if (!performing) return;
        
        subRoot.live.TakeDamage(attackDamage, transform.position, DamageType.Drill, gameObject);
        var damageInfo = LiveMixin.damageInfoPool.Get();
        damageInfo.Clear();
        // Required to update the Cyclops voicelines and call the destruction sequence
        subRoot.live.NotifyAllAttachedDamageReceivers(damageInfo);
        LiveMixin.damageInfoPool.Return(damageInfo);

        performing = false;
    }
}