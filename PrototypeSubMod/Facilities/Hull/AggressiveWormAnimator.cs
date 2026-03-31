using System;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull;

public class AggressiveWormAnimator : ProtoWormAnimator
{
    [SerializeField] private float forwardsSpeed;
    [SerializeField] private float maxRotationSpeed;
    
    private Action onReachedTarget;
    private Vector3 upVector;
    private Vector3 targetPoint;
    private bool stasisPulseFrozen;
    private int segmentsNeededLastFrame;
    private float distMoved;
    private float spineIncrement;
    private float rotationSpeed;
    private float distMovedRecalculate;
    
    protected override void Start()
    {
        base.Start();

        spineIncrement = spineManager.GetIncrementPerSpine().z;
        UpdateRotationSpeed();
        targetPoint = transform.position + transform.forward * forwardsSpeed * 10f;
    }
    
    protected override void Update()
    {
        if (stasisPulseFrozen) return;
        
        transform.position += transform.forward * (forwardsSpeed * Time.deltaTime);
        
        float angleDelta = rotationSpeed * Time.deltaTime;
        transform.forward = Quaternion.AngleAxis(angleDelta, upVector) * transform.forward;

        if (distMoved > distMovedRecalculate)
        {
            rotationSpeed = GetRotationSpeed(transform.position, transform.forward);
        }
        
        distMoved += forwardsSpeed * Time.deltaTime;
        base.Update();
        
        segmentsNeededLastFrame = Mathf.FloorToInt(distMoved / spineIncrement);

        if ((targetPoint - transform.position).sqrMagnitude < 15 * 15)
        {
            onReachedTarget?.Invoke();
        }
    }
    
    protected override void UpdateFollowPoints()
    {
        if (Mathf.FloorToInt(distMoved / spineIncrement) == segmentsNeededLastFrame) return;
        
        Vector3 spawnPoint = transform.position - transform.forward * absIncrement;
        followPoints.Add(new FollowPoint(spawnPoint, transform.rotation));
            
        if (followPoints.Count > spineSegmentsParent.childCount + 1) followPoints.RemoveAt(0);
    }
    
    private void RecalculateUpVector()
    {
        var dirToTarget = targetPoint - transform.position;
        var angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
        
        if (WithinRange(angleToTarget, 180, 0.05f))
        {
            dirToTarget += transform.right * 0.001f;
        }
        
        upVector = Vector3.Cross(dirToTarget, transform.forward);
    }
    
    private float ValidateRotationSpeed(float rotSpeed, float currentDist, out float distRecalculate)
    {
        distRecalculate = Mathf.Infinity;

        float speedSign = Mathf.Sign(rotSpeed);
        float absRotSpeed = Mathf.Abs(rotSpeed);
        if (absRotSpeed > maxRotationSpeed)
        {
            rotSpeed = maxRotationSpeed * 0.5f * speedSign;
            // Recalculate in 2 seconds
            distRecalculate = currentDist + forwardsSpeed * 2f;
        }

        return rotSpeed;
    }
    
    private float GetRotationSpeed(Vector3 currentPosition, Vector3 forward)
    {
        var dirToTarget = targetPoint - currentPosition;
        var angleToTarget = Vector3.Angle(forward, dirToTarget);
        float diameter = Vector3.Distance(currentPosition, targetPoint) / (Mathf.Sin(angleToTarget * Mathf.Deg2Rad) * forwardsSpeed);
        var speed = 1 / (diameter * 8.74e-3f);

        return ValidateRotationSpeed(-speed, distMoved, out distMovedRecalculate);
    }

    private bool WithinRange(float value, float target, float halfRange)
    {
        return value > target - halfRange && value < target + halfRange;
    }

    private float GetAngleToTarget()
    {
        var dirToTarget = targetPoint - transform.position;
        return Vector3.Angle(transform.forward, dirToTarget);
    }

    public void SetTravelTarget(Vector3 point, Action onReachedTarget)
    {
        this.onReachedTarget = onReachedTarget;
        targetPoint = point;
        UpdateRotationSpeed();
    }

    public void OnFreezeByStasisPulse()
    {
        stasisPulseFrozen = true;
    }

    public void OnUnfreezeByStasisPulse()
    {
        stasisPulseFrozen = false;
    }

    public float GetForwardsSpeed() => forwardsSpeed;
    public void SetForwardsSpeed(float speed) => forwardsSpeed = speed;

    private void UpdateRotationSpeed()
    {
        rotationSpeed = GetRotationSpeed(transform.position, transform.forward);

        var angleToTarget = GetAngleToTarget();
        // If almost exactly 180 degrees from the target, turn at max speed for 90 degrees
        if (WithinRange(angleToTarget, 180, 0.05f))
        {
            rotationSpeed = -maxRotationSpeed;
            distMovedRecalculate = distMoved + forwardsSpeed / maxRotationSpeed * 90;
        }
        
        // If the angle is further than we can rotate in a second, go at max speed for half the angle to target
        if (angleToTarget > maxRotationSpeed)
        {
            rotationSpeed = Mathf.Sign(rotationSpeed) * maxRotationSpeed;
            distMovedRecalculate = distMoved + forwardsSpeed / maxRotationSpeed * (angleToTarget * 0.5f);
        }

        RecalculateUpVector();
    }

    public override float GetDistanceMoved() => distMoved;
}