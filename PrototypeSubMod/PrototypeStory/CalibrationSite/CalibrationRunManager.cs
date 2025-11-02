using Nautilus.Utility;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationRunManager : MonoBehaviour
{
    private static readonly Vector3 InitialPoint = new(-2220, -390, 420);

    [SerializeField] private float globalSpacing = 100;
    [SerializeField] private float[] pointSpacings;
    [SerializeField] private float[] relativePointAngles;
    [SerializeField] private float distToCountAsReached = 10;

    private int nextPointIndex = 1;
    private Vector3[] calibrationPoints;

    private void Start()
    {
        // Add one to account for initial point
        calibrationPoints = new Vector3[relativePointAngles.Length + 1];
        calibrationPoints[0] = InitialPoint;
        for (int i = 0; i < relativePointAngles.Length; i++)
        {
            var xComponent = Mathf.Cos(relativePointAngles[i] * Mathf.Deg2Rad);
            var yComponent = Mathf.Sin(relativePointAngles[i] * Mathf.Deg2Rad);
            var offset = new Vector3(xComponent, 0, yComponent) * pointSpacings[i] * globalSpacing;
            calibrationPoints[i + 1] = calibrationPoints[i] + offset;
        }

        foreach (var point in calibrationPoints)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            MaterialUtils.ApplySNShaders(sphere);
            sphere.transform.position = point;
            sphere.transform.localScale = Vector3.one * 10f;
            Destroy(sphere.GetComponent<Collider>());
        }
    }
    
    private void Update()
    {
        var dist = Vector3.Distance(transform.position, calibrationPoints[nextPointIndex]);
        Plugin.Logger.LogInfo($"Dist = {dist}");
        if (dist > distToCountAsReached) return;
        
        ErrorMessage.AddError($"Reached point {nextPointIndex}");
        nextPointIndex++;

        if (nextPointIndex >= calibrationPoints.Length)
        {
            ErrorMessage.AddError("Calibration complete");
            nextPointIndex = calibrationPoints.Length - 1;
        }
    }

    public int GetNextPointIndex() => nextPointIndex;
    public Vector3 GetCalibrationPoint(int index) => calibrationPoints[index];
}