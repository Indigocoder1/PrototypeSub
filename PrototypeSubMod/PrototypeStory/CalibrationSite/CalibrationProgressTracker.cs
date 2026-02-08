using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationProgressTracker : MonoBehaviour, IScheduledUpdateBehaviour
{
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private CalibrationRunManager runManager;
    [SerializeField] private Transform subIcon;
    [SerializeField] private Transform pointNumbersParent;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float positionScalar;
    [SerializeField] private float iconScale;
    
    public string GetProfileTag() => "CalibrationRunProgressTracker";
    public int scheduledUpdateIndex { get; set; }

    private Transform[] locationPoints;
    private int prevPositionCount = -1;
    
    private void Start()
    {
        var calibrationPoints = runManager.GetCalibrationPoints();
        locationPoints = new Transform[calibrationPoints.Length];
        
        for (int i = 0; i < calibrationPoints.Length; i++)
        {
            locationPoints[i] = CreatePoint(i, calibrationPoints[i]);
        }
    }

    private Transform CreatePoint(int index, Vector3 worldPos)
    {
        var pointNumbers = runManager.GetPointNumbers();
        var localPos = WorldSpaceToDisplaySpace(worldPos);

        var symbolObject = pointNumbers[index].CreateSymbolObject();
        symbolObject.transform.SetParent(pointNumbersParent, false);

        symbolObject.transform.localPosition = localPos;
        symbolObject.transform.localScale = Vector3.one * iconScale;
        return symbolObject.transform;
    }

    public void ScheduledUpdate()
    {
        subIcon.transform.localPosition = WorldSpaceToDisplaySpace(subRoot.transform.position);

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