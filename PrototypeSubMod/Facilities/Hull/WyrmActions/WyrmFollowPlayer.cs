using System;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmFollowPlayer : CreatureAction
{
    [SerializeField] private AggressiveWormAnimator wormAnimator;
    [SerializeField] private float offsetFromPlayer;
    [SerializeField] private float timeBetweenPointRecalculations = 15f;

    private Vector3 targetPoint;
    private float recalculationCountdown;

    private void Start()
    {
        RecalculateTargetPoint();
    }

    private void Update()
    {
        if (recalculationCountdown > 0)
        {
            recalculationCountdown -= Time.deltaTime;
        }
        else
        {
            RecalculateTargetPoint();
        }
    }

    private void RecalculateTargetPoint()
    {
        var dir = Player.main.transform.position - transform.position;
        targetPoint = Player.main.transform.position + dir.normalized * offsetFromPlayer;
        wormAnimator.SetTravelTarget(targetPoint, RecalculateTargetPoint);
        recalculationCountdown = timeBetweenPointRecalculations;
        Plugin.Logger.LogInfo($"Recalculating target point on {gameObject}");
    }
}