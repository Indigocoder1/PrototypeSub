using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationProgressTracker : MonoBehaviour, IScheduledUpdateBehaviour
{
    [SerializeField] private CalibrationRunManager runManager;
    [SerializeField] private Transform subIcon;
    [SerializeField] private Transform[] calibrationPoints;
    
    public string GetProfileTag() => "CalibrationRunProgressTracker";
    public int scheduledUpdateIndex { get; set; }
    
    public void ScheduledUpdate()
    {
        int nextIndex = runManager.GetNextPointIndex();
        Vector3 pointA = runManager.GetCalibrationPoint(nextIndex - 1);
        Vector3 pointB = runManager.GetCalibrationPoint(nextIndex);
        float progress = InverseLerp(pointA, pointB, transform.position);

        subIcon.position = Vector3.Lerp(calibrationPoints[nextIndex - 1].position, calibrationPoints[nextIndex].position, progress);
    }
    
    private float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    public virtual void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }

    public virtual void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }
}