using UnityEngine;

public class AngleTest : MonoBehaviour
{
    [SerializeField] private int testRays = 10;
    [Range(0, 90)]
    [SerializeField] private float maxAngleFromForward;
    
    private void OnDrawGizmos()
    {
        var dir = transform.position.normalized;
        Gizmos.DrawRay(transform.position, dir);

        Gizmos.color = Color.green;
        foreach (var testDir in PointsOnSphere(testRays))
        {
            var newDir = testDir;
            newDir *= Mathf.Sign(Vector3.Dot(newDir, dir));
            float angleBetween = Vector3.Angle(newDir, dir);
            newDir = Vector3.RotateTowards(newDir, dir, angleBetween * (1 - maxAngleFromForward / 90) * Mathf.Deg2Rad, 1);
            Gizmos.DrawRay(transform.position, newDir.normalized);
        }
    }
    
    private Vector3[] PointsOnSphere(int num)
    {
        Vector3[] points = new Vector3[num];
        float increment = Mathf.PI * (3 - Mathf.Sqrt(5));
        float offset = 2f / num;

        for (int i = 0; i < num; i++)
        {
            float y = (i * offset) - 1 + (offset / 2);
            float r = Mathf.Sqrt(1 - (y * y));
            float phi = i * increment;
            float x = Mathf.Cos(phi) * r;
            float z = Mathf.Sin(phi) * r;

            points[i] = new Vector3(x, y, z);
        }

        return points;
    }
}
