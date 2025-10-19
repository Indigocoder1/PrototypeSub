using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull;

public class AggressiveWormAnimator : ProtoWormAnimator
{
    [SerializeField] private float speed;
    
    private float distMoved;
    private float spineIncrement;
    private int segmentsNeededLastFrame;
    
    protected override void Start()
    {
        base.Start();

        spineIncrement = spineManager.GetIncrementPerSpine().z;
    }
    
    protected override void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
        distMoved += speed * Time.deltaTime;
        
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