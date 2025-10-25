using System;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull;

public class AggressiveWormAnimator : ProtoWormAnimator
{
    [SerializeField] private float forwardsSpeed;
    [SerializeField] private float maxRotationSpeed;
    
    private float distMoved;
    private float spineIncrement;
    private int segmentsNeededLastFrame;
    private Action onReachedTarget;
    private Vector3 upVector;
    private Vector3 targetPoint;
    private float rotationSpeed;
    private float prevAngleToTarget;
    private float angleTravelled;
    private float angleTravelledRecalculate;
    
    protected override void Start()
    {
        base.Start();

        spineIncrement = spineManager.GetIncrementPerSpine().z;
        UpdateRotationSpeed();
        targetPoint = transform.position + transform.forward * forwardsSpeed * 10f;
    }
    
    protected override void Update()
    {
        var dirToTarget = targetPoint - transform.position;
        var angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
        
        transform.position += transform.forward * (forwardsSpeed * Time.deltaTime);
        
        float angleDelta = rotationSpeed * Time.deltaTime;
        transform.forward = Quaternion.AngleAxis(angleDelta, upVector) * transform.forward;
        angleTravelled += Mathf.Abs(angleDelta);

        if (angleTravelled > angleTravelledRecalculate)
        {
            rotationSpeed = GetRotationSpeed(transform.position, transform.forward);
            rotationSpeed = ValidateRotationSpeed(rotationSpeed, angleTravelled, out angleTravelledRecalculate);
        }
        
        distMoved += forwardsSpeed * Time.deltaTime;
        base.Update();
        
        segmentsNeededLastFrame = Mathf.FloorToInt(distMoved / spineIncrement);

        if ((targetPoint - transform.position).sqrMagnitude < 15 * 15)
        {
            onReachedTarget?.Invoke();
        }

        prevAngleToTarget = angleToTarget;
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
    
    private float ValidateRotationSpeed(float rotSpeed, float currentAngle, out float angleRecalculate)
    {
        angleRecalculate = Mathf.Infinity;

        float speedSign = Mathf.Sign(rotSpeed);
        float absRotSpeed = Mathf.Abs(rotSpeed);
        if (absRotSpeed > maxRotationSpeed)
        {
            rotSpeed = maxRotationSpeed * 0.5f * speedSign;
            angleRecalculate = currentAngle + maxRotationSpeed * 2;
        }

        return rotSpeed;
    }
    
    private float GetRotationSpeed(Vector3 currentPosition, Vector3 forward)
    {
        var dirToTarget = targetPoint - currentPosition;
        var angleToTarget = Vector3.Angle(forward, dirToTarget);
        float diameter = Vector3.Distance(currentPosition, targetPoint) / (Mathf.Sin(angleToTarget * Mathf.Deg2Rad) * forwardsSpeed);
        var speed = 1 / (diameter * 8.74e-3f);

        return -speed;
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

    private void UpdateRotationSpeed()
    {
        rotationSpeed = GetRotationSpeed(transform.position, transform.forward);
        rotationSpeed = ValidateRotationSpeed(rotationSpeed, 0, out _);

        var angleToTarget = GetAngleToTarget();
        if (WithinRange(angleToTarget, 180, 0.05f))
        {
            rotationSpeed = -maxRotationSpeed;
            angleTravelledRecalculate = maxRotationSpeed * 2;
        }
        
        if (angleToTarget > maxRotationSpeed)
        {
            rotationSpeed = Mathf.Sign(rotationSpeed) * maxRotationSpeed;
            angleTravelledRecalculate = angleToTarget;
        }

        RecalculateUpVector();
    }

    public override float GetDistanceMoved() => distMoved;
}