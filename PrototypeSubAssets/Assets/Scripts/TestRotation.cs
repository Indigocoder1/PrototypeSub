using UnityEngine;

public class TestRotation : MonoBehaviour
{
    [SerializeField] private float forwardsSpeed;
    [SerializeField] private float maxRotationSpeed;
    [SerializeField] private Transform target;
    [SerializeField] private int gizmoSteps;
    [SerializeField] private float gizmoTimestep = 1;

    private Vector3 upVector;
    private float rotationSpeed;
    private float angleTravelled;
    private float angleTravelledRecalculate;

    private void Start()
    {
        rotationSpeed = GetRotationSpeed(transform.position, transform.forward);
        var dirToTarget = target.position - transform.position;
        var angleToTarget = Vector3.Angle(transform.forward, dirToTarget);

        rotationSpeed = ValidateRotationSpeed(rotationSpeed, 0, out _);
        
        if (WithinRange(angleToTarget, 180, 0.05f))
        {
            rotationSpeed = -maxRotationSpeed;
            angleTravelledRecalculate = maxRotationSpeed * 2;
        }
        
        if (Mathf.Abs(rotationSpeed) < maxRotationSpeed && angleToTarget > 90f)
        {
            rotationSpeed = Mathf.Sign(rotationSpeed) * maxRotationSpeed;
            angleTravelledRecalculate = (angleToTarget - 10) / maxRotationSpeed;
        }

        RecalculateUpVector();
    }

    private void Update()
    {
        transform.position += transform.forward * forwardsSpeed * Time.deltaTime;
        float angleDelta = rotationSpeed * Time.deltaTime;
            
        transform.forward = Quaternion.AngleAxis(angleDelta, upVector) * transform.forward;
        angleTravelled += Mathf.Abs(angleDelta);

        if (angleTravelled > angleTravelledRecalculate)
        {
            rotationSpeed = GetRotationSpeed(transform.position, transform.forward);
            rotationSpeed = ValidateRotationSpeed(rotationSpeed, angleTravelled, out angleTravelledRecalculate);
        }
    }

    private void OnDrawGizmos()
    {
        var rotSpeed = GetRotationSpeed(transform.position, transform.forward);
        var dirToTarget = target.position - transform.position;
        var angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
        var point = transform.position;
        var direction = transform.forward;

        rotSpeed = ValidateRotationSpeed(rotSpeed, 0, out var angleRecalculate);
        
        if (WithinRange(angleToTarget, 180, 0.05f))
        {
            rotSpeed = -maxRotationSpeed;
            angleRecalculate = maxRotationSpeed * 2;
            dirToTarget += transform.right * 0.001f;
        }
        
        if (angleToTarget > maxRotationSpeed)
        {
            rotSpeed = Mathf.Sign(rotSpeed) * maxRotationSpeed;
            angleRecalculate = angleToTarget;
        }
        
        Gizmos.color = Color.white;
        bool colorSwitched = false;
        float travelledAngle = 0;
        for (int i = 0; i < gizmoSteps; i++)
        {
            Gizmos.DrawRay(point, direction * forwardsSpeed * gizmoTimestep);
            point += direction * forwardsSpeed * gizmoTimestep;
            float angleDelta = rotSpeed * gizmoTimestep;
            var up = Vector3.Cross(dirToTarget, transform.forward);
            
            direction = Quaternion.AngleAxis(angleDelta, up) * direction;
            travelledAngle += Mathf.Abs(angleDelta);

            if (travelledAngle > angleRecalculate)
            {
                rotSpeed = GetRotationSpeed(point, direction);
                rotSpeed = ValidateRotationSpeed(rotSpeed, travelledAngle, out angleRecalculate);
                Gizmos.color = colorSwitched ? Color.green : Color.blue;
                colorSwitched = !colorSwitched;
            }
        }
    }

    private void RecalculateUpVector()
    {
        var dirToTarget = target.position - transform.position;
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
        var dirToTarget = target.position - currentPosition;
        var angleToTarget = Vector3.Angle(forward, dirToTarget);
        float diameter = Vector3.Distance(currentPosition, target.position) / (Mathf.Sin(angleToTarget * Mathf.Deg2Rad) * forwardsSpeed);
        var speed = 1 / (diameter * 8.74e-3f);

        return -speed;
    }

    private bool WithinRange(float value, float target, float halfRange)
    {
        return value > target - halfRange && value < target + halfRange;
    }
}
