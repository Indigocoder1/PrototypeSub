using System;
using System.Collections.Generic;
using Nautilus.Utility;
using PrototypeSubMod.LightDistortionField;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmDartAction : CreatureAction
{
    [SerializeField] private AggressiveWormAnimator wormAnimator;
    [SerializeField] private WyrmRoarManager roarManager;
    [SerializeField] private float increasedSpeed;
    [SerializeField] private FMOD_CustomEmitter dartSpecialSFX;

    private Transform target;
    private CloakEffectHandler targetCloakHandler;
    private Vector3[] movementPoints;
    private bool performing;
    private bool speedIncreased;
    private float originalSpeed;
    private int rightHandSign;
    private int attackStage;

    public override float Evaluate(Creature creature, float time)
    {
        return performing ? 1 : Random.Range(0f, 0.8f);
    }
    
    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;
        
        Plugin.Logger.LogInfo("Starting dart action");
        
        base.Perform(creature, time, deltaTime);
        performing = true;
        speedIncreased = false;
        attackStage = 0;
        rightHandSign = (int)Mathf.Sign(Random.Range(-1f, 1f));
        
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

        targetCloakHandler = target.GetComponentInChildren<CloakEffectHandler>();

        originalSpeed = wormAnimator.GetForwardsSpeed();
        wormAnimator.SetTravelTarget(GetMovementPoints()[attackStage], OnPointReached);
    }
    
    public void OverrideStopPerform()
    {
        performing = false;
    }


    private void OnPointReached()
    {
        attackStage++;
        dartSpecialSFX.Stop();
        
        if (attackStage >= GetMovementPoints().Length)
        {
            performing = false;
            wormAnimator.SetForwardsSpeed(originalSpeed);
        }
    }

    private void Update()
    {
        if (!performing) return;

        var tempPoints = GetMovementPoints();
        if (attackStage < tempPoints.Length - 2)
        {
            movementPoints = tempPoints;
        }

        var angle = Vector3.Angle(transform.forward, movementPoints[attackStage] - transform.position);
        if (attackStage == 3 && angle < 25 && !speedIncreased)
        {
            wormAnimator.SetForwardsSpeed(increasedSpeed);
            roarManager.PlayRoar(Player.main.transform.position);
            speedIncreased = true;

            var random = Random.Range(0f, 500f);
            if (random < 1)
            {
                dartSpecialSFX.Play();
            }

        }
        
        wormAnimator.SetTravelTarget(movementPoints[attackStage], OnPointReached);
    }

    private Vector3[] GetMovementPoints()
    {
        var points = new Vector3[5];
        const float setupOffset = 200;
        points[0] = target.position + (target.right * -rightHandSign - target.forward).normalized * setupOffset;
        if (targetCloakHandler != null)
        {
            points[1] = target.position - target.forward * setupOffset;
            points[2] = targetCloakHandler.GetClosestPointOnSurface(target.position + target.right * (rightHandSign * setupOffset), setupOffset / 2f);
            points[3] = targetCloakHandler.GetClosestPointOnSurface(target.position + target.forward * setupOffset);
            points[4] = points[3] - target.right * (rightHandSign * setupOffset * 2f);
        }
        else
        {
            points[1] = target.position - target.forward * setupOffset;
            points[2] = target.position - target.right * setupOffset;
            points[3] = target.position + target.forward * 10f;
            points[4] = points[3] - target.right * (rightHandSign * setupOffset * 2f);
        }

        return points;
    }
}