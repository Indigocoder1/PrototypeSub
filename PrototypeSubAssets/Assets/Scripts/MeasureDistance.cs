using UnityEngine;

public class MeasureDistance : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    private void OnDrawGizmos()
    {
        if (!pointA || !pointB) return;

        Debug.Log(Vector3.Distance(pointA.position, pointB.position));
    }
}
