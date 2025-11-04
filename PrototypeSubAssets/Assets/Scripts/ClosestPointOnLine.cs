using PrototypeSubMod.Teleporter;
using UnityEditor;
using UnityEngine;

public class ClosestPointOnLine : MonoBehaviour
{
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public float sphereSize;
    public float maxDistFromLine;
    
    private void OnDrawGizmos()
    {
        if (!point1 || !point2 || !point3) return;

        var heading = point2.position - point1.position;
        var maxMagnitude = heading.magnitude;
        heading.Normalize();

        Gizmos.DrawLine(point1.position, point2.position);
        
        var lhs = point3.position - point1.position;
        var dot = Vector3.Dot(lhs, heading);
        dot = Mathf.Clamp(dot, 0f, maxMagnitude);
        Gizmos.DrawSphere(point1.position, 0.1f);
        Gizmos.DrawSphere(point2.position, 0.1f);
        Gizmos.DrawSphere(point3.position, 0.1f);
        
        var point = point1.position + heading * dot;
        Gizmos.DrawLine(point, point3.position);
        
        Gizmos.color = Color.green;
        if (Vector3.Distance(point, point3.position) > maxDistFromLine)
        {
            Gizmos.color = Color.red;
        }
        
        Gizmos.DrawSphere(point, sphereSize);
    }
}
