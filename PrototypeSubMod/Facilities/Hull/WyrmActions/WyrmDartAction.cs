using System;
using System.Collections.Generic;
using Nautilus.Utility;
using PrototypeSubMod.LightDistortionField;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmDartAction : WyrmAction
{
    [SerializeField] private WyrmRoarManager roarManager;
    [SerializeField] private float increasedSpeed;
    [SerializeField] private float maxDistMovedToRecalculate;
    [SerializeField] private FMOD_CustomEmitter dartSpecialSFX;

    private Transform target;
    private CloakEffectHandler targetCloakHandler;
    private Vector3 targetPointWhenStartedPath;
    private bool speedIncreased;
    private float originalSpeed;
    private int rightHandSign;

    private void Start()
    {
        OnReachedTarget += OnPointReached;
    }
    
    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;
        
        Plugin.Logger.LogInfo("Starting dart action");
        
        SetupTargetTransform();
        base.Perform(creature, time, deltaTime);
        speedIncreased = false;
        rightHandSign = (int)Mathf.Sign(Random.Range(-1f, 1f));
        rightHandSign = rightHandSign == 0 ? 1 : rightHandSign;
        
        targetCloakHandler = target.GetComponentInChildren<CloakEffectHandler>();
        originalSpeed = wormAnimator.GetForwardsSpeed();
        targetPointWhenStartedPath = target.position;
    }

    private void SetupTargetTransform()
    {
        var player = Player.main;
        if (player.currentSub)
        {
            target = player.currentSub.transform;
        }
        else if (player.lastValidSub &&
                 Vector3.Distance(player.lastValidSub.transform.position, player.transform.position) < 50f)
        {
            target = player.lastValidSub.transform;
        }
        else
        {
            target = player.transform;
        }
    }
    
    private void OnPointReached()
    {
        dartSpecialSFX.Stop();
        
        if (AttackStage >= GetMovementPoints().Length)
        {
            wormAnimator.SetForwardsSpeed(originalSpeed);
        }

        targetPointWhenStartedPath = target.position;
    }

    private void Update()
    {
        if (!performing) return;

        if (Vector3.Distance(target.position, targetPointWhenStartedPath) > maxDistMovedToRecalculate)
        {
            wormAnimator.SetTravelTarget(GetMovementPoints()[AttackStage], OnReachedTargetPoint);
            targetPointWhenStartedPath = target.position;
        }
        
        HandleSpeedIncrease();
    }

    private void HandleSpeedIncrease()
    {
        var movementPoints = GetMovementPoints();
        // Don't update anything if past the speed-up stage
        if (AttackStage >= movementPoints.Length - 1)
        {
            return;
        }
        
        var angle = Vector3.Angle(transform.forward, movementPoints[AttackStage] - transform.position);
        if (AttackStage != 3 || !(angle < 20) || speedIncreased) return;
        
        wormAnimator.SetForwardsSpeed(increasedSpeed);
        roarManager.PlayRoar(Player.main.transform.position);
        speedIncreased = true;
        
        if (Random.Range(0f, 1000f) < 1)
        {
            dartSpecialSFX.Play();
        }

        wormAnimator.SetTravelTarget(movementPoints[AttackStage], OnReachedTargetPoint);
        targetPointWhenStartedPath = target.position;
    }

    protected override Vector3[] GetMovementPoints()
    {
        var points = new Vector3[5];
        const float setupOffset = 300;
        points[0] = target.position + (target.right * -rightHandSign - target.forward).normalized * setupOffset;
        if (targetCloakHandler != null)
        {
            points[1] = target.position - target.forward * setupOffset;
            points[2] = targetCloakHandler.GetClosestPointOnSurface(target.position + target.right * (rightHandSign * setupOffset), setupOffset / 2f);
            points[3] = targetCloakHandler.GetClosestPointOnSurface(target.position + target.forward * (setupOffset));
            points[4] = points[3] - target.right * (rightHandSign * setupOffset);
        }
        else
        {
            points[1] = target.position - target.forward * setupOffset;
            points[2] = target.position + target.right * (rightHandSign * setupOffset);
            points[3] = target.position + target.forward * 10f;
            points[4] = points[3] - target.right * (rightHandSign * setupOffset);
        }

        return points;
    }
}