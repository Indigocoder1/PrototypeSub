using System;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull;

public class CinematicWormAnimator : ProtoWormAnimator
{
    [SerializeField] private float forwardsSpeed;
    
    private Vector3 posLastFrame;
    private float spineIncrement;
    private float distMoved;
    private int segmentsNeededLastFrame;

    protected override void Start()
    {
        posLastFrame = transform.position;
        spineIncrement = spineManager.GetIncrementPerSpine().z;
        base.Start();
    }

    protected override void Update()
    {
        distMoved += forwardsSpeed * Time.deltaTime;
        
        base.Update();
        
        segmentsNeededLastFrame = Mathf.FloorToInt(distMoved / spineIncrement);
    }

    protected override void UpdateFollowPoints()
    {
        if (Mathf.FloorToInt(distMoved / spineIncrement) == segmentsNeededLastFrame) return;
        
        Vector3 spawnPoint = transform.position - transform.forward * absIncrement;
        followPoints.Add(new FollowPoint(spawnPoint, transform.rotation));
            
        if (followPoints.Count > spineSegmentsParent.childCount + 1) followPoints.RemoveAt(0);
    }

    public override float GetDistanceMoved() => distMoved;
}