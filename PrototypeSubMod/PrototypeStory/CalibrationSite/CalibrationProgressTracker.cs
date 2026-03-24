using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationProgressTracker : MonoBehaviour, IScheduledUpdateBehaviour
{
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private CalibrationRunManager runManager;
    [SerializeField] private Transform subIcon;
    [SerializeField] private Transform[] locationPoints;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float positionScalar;
    
    public string GetProfileTag() => "CalibrationRunProgressTracker";
    public int scheduledUpdateIndex { get; set; }
    
    private int prevPositionCount = -1;

    public void ScheduledUpdate()
    {
        subIcon.transform.localPosition = WorldSpaceToDisplaySpace(subRoot.transform.position);
        var eulerAngles = subIcon.transform.localEulerAngles;
        subIcon.transform.localEulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, -subRoot.transform.localEulerAngles.y);
        
        var positionCount = runManager.GetNextIndex() + 1;
        lineRenderer.positionCount = positionCount;
        lineRenderer.SetPosition(positionCount - 1, subIcon.transform.localPosition);

        if (prevPositionCount == positionCount) return;

        var positions = new Vector3[positionCount];
        for (int i = 0; i < positionCount - 1; i++)
        {
            positions[i] = locationPoints[i].localPosition;
        }

        positions[positionCount - 1] = subIcon.transform.localPosition;
        lineRenderer.SetPositions(positions);
        
        prevPositionCount = positionCount;
    }

    public void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }

    public void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }

    private Vector3 WorldSpaceToDisplaySpace(Vector3 position)
    {
        var localPos = position - runManager.GetSiteCenter();
        localPos /= positionScalar;
        return new Vector3(localPos.x, localPos.z);
    }
}