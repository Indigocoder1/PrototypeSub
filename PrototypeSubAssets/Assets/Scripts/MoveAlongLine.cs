using UnityEngine;

public class MoveAlongLine : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform movingPoint;

    private void OnDrawGizmosSelected()
    {
        if (!pointA || !pointB || !movingPoint) return;

        Gizmos.DrawLine(pointA.position, pointB.position);
        movingPoint.transform.position =
            Vector3.Project(movingPoint.transform.position - pointA.position, pointB.position - pointA.position) + pointA.position;
    }
}
