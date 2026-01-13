using System;
using Nautilus.Utility;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationRunManager : MonoBehaviour, IScheduledUpdateBehaviour
{
    private static readonly Vector3 InitialPoint = new(-2220, -390, 420);

    public event Action<int> onPointReached;

    [SerializeField] private GameObject calibrationObjects;
    [SerializeField] private float globalSpacing = 100;
    [SerializeField] private float[] pointSpacings;
    [SerializeField] private float[] relativePointAngles;
    [SerializeField] private float distToCountAsReached = 10;
    [SerializeField] private float maxDistanceFromCenter;

    private bool doingCalibrationRun;
    private int nextPointIndex = 1;
    private Vector3[] calibrationPoints;
    private Vector3 pointsCenter;

    private void Start()
    {
        // Add one to account for initial point
        calibrationPoints = new Vector3[relativePointAngles.Length + 1];
        calibrationPoints[0] = InitialPoint;
        pointsCenter = InitialPoint;
        for (int i = 0; i < relativePointAngles.Length; i++)
        {
            var xComponent = Mathf.Cos(relativePointAngles[i] * Mathf.Deg2Rad);
            var yComponent = Mathf.Sin(relativePointAngles[i] * Mathf.Deg2Rad);
            var offset = new Vector3(xComponent, 0, yComponent) * pointSpacings[i] * globalSpacing;
            calibrationPoints[i + 1] = calibrationPoints[i] + offset;
            pointsCenter += calibrationPoints[i + 1];
        }

        pointsCenter /= relativePointAngles.Length + 1; 

        int index = 0;
        foreach (var point in calibrationPoints)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            MaterialUtils.ApplySNShaders(sphere);
            sphere.transform.position = point;
            sphere.transform.localScale = Vector3.one * 10f;
            Destroy(sphere.GetComponent<Collider>());
            
            if (index == calibrationPoints.Length - 1) continue;
            
            var lr = sphere.AddComponent<LineRenderer>();
            lr.SetPosition(0, calibrationPoints[index]);
            lr.SetPosition(1, calibrationPoints[index + 1]);
            index++;
        }

        calibrationObjects.SetActive(false);
    }
    
    private void Update()
    {
        if (!doingCalibrationRun) return;
        
        HandleIndexIncrements();
        HandleDistanceFromCenter();
    }

    private void HandleDistanceFromCenter()
    {
        if (GetNormalizedDistFromCenter() < 1) return;
        
        ErrorMessage.AddError("Too far from line! Failed calibration run");
        doingCalibrationRun = false;
        nextPointIndex = 1;
        calibrationObjects.SetActive(false);
    }

    private void HandleIndexIncrements()
    {
        var dist = Vector3.Distance(transform.position, calibrationPoints[nextPointIndex]);
        if (dist > distToCountAsReached) return;
        
        ErrorMessage.AddError($"Reached point {nextPointIndex}");
        onPointReached?.Invoke(nextPointIndex);
        nextPointIndex++;

        if (nextPointIndex >= calibrationPoints.Length)
        {
            ErrorMessage.AddError("Calibration complete");
            nextPointIndex = 1;
            doingCalibrationRun = false;
            calibrationObjects.SetActive(false);
        }
    }

    public float GetNormalizedDistFromCenter()
    {
        return Vector3.Distance(transform.position, pointsCenter) / maxDistanceFromCenter;
    }

    public int GetNextPointIndex() => nextPointIndex;
    public Vector3 GetCalibrationPoint(int index) => calibrationPoints[index];
    public Vector3 GetSiteCenter() => pointsCenter;
    
    public void ScheduledUpdate()
    {
        if (doingCalibrationRun) return;
        
        if (!(Vector3.Distance(calibrationPoints[0], transform.position) < distToCountAsReached)) return;
        
        doingCalibrationRun = true;
        ErrorMessage.AddError("Started calibration run");
        onPointReached?.Invoke(0);
        calibrationObjects.SetActive(true);
    }

    public string GetProfileTag() => "CalibrationRunManager";
    public int scheduledUpdateIndex { get; set; }
    
    public void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }

    public void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }
}