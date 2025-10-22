using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull;

public class AggressiveWormAnimator : ProtoWormAnimator
{
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float rotationAmplitude;
    
    private float distMoved;
    private float spineIncrement;
    private int segmentsNeededLastFrame;
    
    protected override void Start()
    {
        base.Start();

        spineIncrement = spineManager.GetIncrementPerSpine().z;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x - rotationAmplitude,
            transform.localEulerAngles.y, transform.localEulerAngles.z);
    }
    
    protected override void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
        var angle = (Mathf.Sin(Time.time * rotationSpeed * Mathf.Deg2Rad)) * rotationAmplitude;
        transform.Rotate(transform.right, angle * Time.deltaTime, Space.Self);
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