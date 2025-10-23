using System;
using UnityEngine;

public class TestRotation : MonoBehaviour
{
    [SerializeField] private float forwardsSpeed;
    [SerializeField] private Transform target;
    [SerializeField] private int gizmoSteps;
    [SerializeField] private float gizmoTimestep = 1;
    
    private float rotationSpeed;
    private float angleTravelled;
    private float angleTravelledRecalculate;

    private void Start()
    {
        rotationSpeed = GetRotationSpeed(transform.position, transform.right, out var angleToTarget);
        float distToTarget = Vector3.Distance(transform.position, target.position);
        
        angleTravelledRecalculate = Mathf.Infinity;
        if (WithinRange(angleToTarget, 180, 0.025f))
        {
            rotationSpeed = distToTarget * 10;
            angleTravelledRecalculate = 90;
        }
        Debug.Log(rotationSpeed);
    }

    private void Update()
    {
        transform.position += transform.forward * (forwardsSpeed * Time.deltaTime);
        transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0), Space.Self);
        angleTravelled += rotationSpeed * Time.deltaTime;
        
        if (Mathf.Abs(angleTravelled) >= angleTravelledRecalculate)
        {
            rotationSpeed = GetRotationSpeed(transform.position, transform.right, out _);
            angleTravelledRecalculate = Mathf.Infinity;
        }
    }

    private void OnDrawGizmos()
    {
        var rotationSpeed = GetRotationSpeed(transform.position, transform.right, out var angleToTarget);
        float distToTarget = Vector3.Distance(transform.position, target.position);
        float diameter = distToTarget / (Mathf.Sin(angleToTarget * Mathf.Deg2Rad) * forwardsSpeed);
        var sign = Mathf.Sign(Vector3.Dot(target.position - transform.position, transform.right));
        Gizmos.DrawSphere(transform.position + transform.right * diameter * forwardsSpeed / 2 * sign, 0.2f);
        var point = transform.position;
        var direction = transform.forward;
        
        float angleTravelledRecalculate = Mathf.Infinity;
        if (WithinRange(angleToTarget, 180, 0.025f))
        {
            rotationSpeed = distToTarget * 10;
            angleTravelledRecalculate = 90;
        }

        float angleTravelled = 0;
        for (int i = 0; i < gizmoSteps; i++)
        {
            Gizmos.DrawRay(point, direction * forwardsSpeed * gizmoTimestep);
            point += direction * forwardsSpeed * gizmoTimestep;
            float angleDelta = rotationSpeed * gizmoTimestep;
            direction = Quaternion.AngleAxis(angleDelta, transform.up) * direction;
            angleTravelled += angleDelta;
            
            if (Mathf.Abs(angleTravelled) >= angleTravelledRecalculate)
            {
                Gizmos.color = Color.green;
                rotationSpeed = GetRotationSpeed(point, -Vector3.Cross(direction, transform.up), out _);
            }
        }
    }

    private float GetRotationSpeed(Vector3 currentPosition, Vector3 rightHandNormal, out float angleToTarget)
    {
        var dirToTarget = target.position - currentPosition;
        angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
        float diameter = Vector3.Distance(currentPosition, target.position) / (Mathf.Sin(angleToTarget * Mathf.Deg2Rad) * forwardsSpeed);
        var speed = 1 / (diameter * 8.74e-3f);
        speed *= Mathf.Sign(Vector3.Dot(dirToTarget, rightHandNormal));

        return speed;
    }

    private bool WithinRange(float value, float target, float halfRange)
    {
        return value > target - halfRange && value < target + halfRange;
    }
}
