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
    [SerializeField] private float increasedSpeed;

    private Transform target;
    private CloakEffectHandler targetCloakHandler;
    private bool performing;
    private float originalSpeed;
    private int rightHandSign;
    private int attackStage;

    private List<GameObject> points = new();

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

        foreach (var p in points)
        {
            Destroy(p);
        }

        points.Clear();
        
        foreach (var point in GetMovementPoints())
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = point;
            sphere.transform.localScale = Vector3.one * 5f;
            MaterialUtils.ApplySNShaders(sphere);
            points.Add(sphere);
        }
    }

    private void OnPointReached()
    {
        attackStage++;

        if (attackStage == 3)
        {
            wormAnimator.SetForwardsSpeed(increasedSpeed);
        }
        
        if (attackStage >= GetMovementPoints().Length)
        {
            performing = false;
            wormAnimator.SetForwardsSpeed(originalSpeed);
            return;
        }
        
        wormAnimator.SetTravelTarget(GetMovementPoints()[attackStage], OnPointReached);
    }

    private void Update()
    {
        if (!performing) return;
        
        int i = 0;
        foreach (var point in GetMovementPoints())
        {
            points[i].transform.position = point;
            i++;
        }
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
            points[3] = targetCloakHandler.GetClosestPointOnSurface(target.position + target.forward * (rightHandSign * setupOffset));
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