using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull;

public class PassiveWormAnimator : ProtoWormAnimator
{
    [SerializeField] private GameObject headObject;
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [Range(0, 1)]
    [SerializeField] private float gizmoDrawLength;

    private float distMoved;
    private float endDistance;
    private float fullyDisabledDistance;
    private float spineIncrement;
    private int segmentsNeededLastFrame;

    protected override void Start()
    {
        base.Start();

        spineIncrement = spineManager.GetIncrementPerSpine().z;
        endDistance = speed * 36;
        fullyDisabledDistance = endDistance + absIncrement * spineManager.GetSpineSegmentCount();
    }

    protected override void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
        distMoved += speed * Time.deltaTime;
        transform.Rotate(new Vector3(rotationSpeed * Time.deltaTime, 0, 0), Space.Self);
        
        base.Update();

        if (distMoved >= endDistance)
        {
            headObject.SetActive(false);
        }
        
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

    protected override bool GetShouldUpdateSpineSegment(Transform child, int index)
    {
        if (distMoved + spineIncrement * index >= endDistance)
        {
            child.gameObject.SetActive(false);
            return false;
        }
        
        return base.GetShouldUpdateSpineSegment(child, index);
    }
    
    public float GetRotationSpeed() => rotationSpeed;
    public void SetRotationSpeed(float speed) => rotationSpeed = speed;
    
    // Add a little extra distance to make sure it's fully done
    public bool DoneRotating() => distMoved >= fullyDisabledDistance + speed;
    public bool HeadIsDisabled() => distMoved >= endDistance;
    public float GetRotationDuration() => (spineManager.GetSpineSegmentCount() + 1) * -spineIncrement / speed;
    
    private void OnDrawGizmosSelected()
    {
        Vector3 point = transform.position;
        Vector3 rotation = transform.forward;
        bool test = Vector3.Dot(Quaternion.AngleAxis(speed, transform.right) * transform.forward, Vector3.up) <
                    Vector3.Dot(Quaternion.AngleAxis(-speed, transform.right) * transform.forward, Vector3.up);
        Gizmos.color = test ? Color.green : Color.red;
        int increments = Mathf.FloorToInt(360 / rotationSpeed * gizmoDrawLength);
        for (int i = 0; i < increments; i++)
        {
            Gizmos.DrawRay(point, rotation * speed);
            point += rotation * speed;
            rotation = Quaternion.AngleAxis(rotationSpeed, transform.right) * rotation;
        }
    }
}