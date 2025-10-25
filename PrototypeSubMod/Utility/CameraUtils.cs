using UnityEngine;

namespace PrototypeSubMod.Utility;

internal class ProtoCameraUtils
{
    public static Vector2 CalculateTargetAngleDelta(Transform target, float maxSpeed)
    {
        return CalculateTargetAngleDelta(target.position, maxSpeed);
    }
    
    public static Vector2 CalculateTargetAngleDelta(Vector3 point, float maxSpeed)
    {
        var targetAngles = Quaternion.LookRotation(point - Camera.main.transform.position);

        Vector3 euler1 = targetAngles.eulerAngles;
        Vector3 euler2 = Camera.main.transform.eulerAngles;

        float deltaX = Mathf.DeltaAngle(euler1.x, euler2.x);
        float deltaY = Mathf.DeltaAngle(euler1.y, euler2.y);

        Vector2 cameraDeltaValues = new Vector2(-deltaY, deltaX);

        return Vector2.ClampMagnitude(cameraDeltaValues, maxSpeed);
    }
}
