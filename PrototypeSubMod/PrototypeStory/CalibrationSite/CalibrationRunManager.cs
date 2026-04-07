using System;
using Nautilus.Utility;
using PrototypeSubMod.Puzzles.BearingPuzzle;
using Story;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationRunManager : MonoBehaviour, IScheduledUpdateBehaviour
{
    public static readonly Vector3 InitialPoint = new(-2720, -390, 420);

    public static event Action<int> OnPointReached;
    public static event Action OnCalibrationFailed;
    public static event Action OnCalibrationCompleted;
    
    [SerializeField] private GameObject calibrationObjects;
    [SerializeField] private GameObject calibrationPointPrefab;
    [SerializeField] private BearingReferenceSymbol[] pointNumbers;
    [SerializeField] private float globalSpacing = 100;
    [SerializeField] private float[] pointSpacings;
    [SerializeField] private float[] relativePointAngles;
    [SerializeField] private float distToCountAsReached = 10;
    [SerializeField] private float maxDistanceFromCenter;
    
    [Header("SFX")]
    [SerializeField] private VoiceNotificationManager voiceNotificationManager;
    [SerializeField] private VoiceNotification reachedPointVoiceline;
    [SerializeField] private VoiceNotification failedCalibrationVoiceline;
    [SerializeField] private VoiceNotification startedCalibrationVoiceline;
    
    private bool doingCalibrationRun;
    private int nextPointIndex = 1;
    private Vector3[] calibrationPoints;
    private Vector3 pointsCenter;

    private void Start()
    {
        calibrationObjects.SetActive(false);
        
        if (StoryGoalManager.main.IsGoalComplete("OnCalibrationRunCompleted")) return;
        
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
            var calibrationPointObject = Instantiate(calibrationPointPrefab, point, Quaternion.identity);
            var calibrationPoint = calibrationPointObject.GetComponent<CalibrationPoint>();
            calibrationPoint.SetBearingReference(pointNumbers[index]);
            
            index++;
        }
    }
    
    private void Update()
    {
        if (!doingCalibrationRun) return;
        
        HandleWrongPointFailure();
        HandleIndexIncrements();
        HandleDistanceFromCenter();
    }

    private void HandleDistanceFromCenter()
    {
        if (GetNormalizedDistFromCenter() < 1) return;
        
        FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("EngineScream"), transform.position);
        doingCalibrationRun = false;
        nextPointIndex = 1;
        calibrationObjects.SetActive(false);
        OnCalibrationFailed?.Invoke();
        voiceNotificationManager.PlayVoiceNotification(failedCalibrationVoiceline);
    }

    private void HandleIndexIncrements()
    {
        var dist = Vector3.Distance(transform.position, calibrationPoints[nextPointIndex]);
        if (dist > distToCountAsReached) return;
        
        OnPointReached?.Invoke(nextPointIndex);
        nextPointIndex++;

        if (nextPointIndex < calibrationPoints.Length)
        {
            voiceNotificationManager.PlayVoiceNotification(reachedPointVoiceline);
            FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("DefenseDoorSignal_Searching"), transform.position);
            return;
        }
        
        FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("DefenseDoorSignal_Found"), transform.position);
        nextPointIndex = 1;
        doingCalibrationRun = false;
        calibrationObjects.SetActive(false);
        StoryGoalManager.main.OnGoalComplete("OnCalibrationRunCompleted");
        OnCalibrationCompleted?.Invoke();
    }

    private void HandleWrongPointFailure()
    {
        for (int i = 0; i < calibrationPoints.Length; i++)
        {
            if (i == nextPointIndex || i == nextPointIndex - 1) continue;
            
            if ((transform.position - calibrationPoints[i]).sqrMagnitude > distToCountAsReached * distToCountAsReached) continue;

            ErrorMessage.AddError("Wrong point reached! Calibration failed");
            doingCalibrationRun = false;
            nextPointIndex = 1;
            calibrationObjects.SetActive(false);
            OnCalibrationFailed?.Invoke();
            voiceNotificationManager.PlayVoiceNotification(failedCalibrationVoiceline);

            break;
        }
    }

    public float GetNormalizedDistFromCenter()
    {
        return Vector3.Distance(transform.position, pointsCenter) / maxDistanceFromCenter;
    }
    
    public Vector3 GetSiteCenter() => pointsCenter;
    public Vector3[] GetCalibrationPoints() => calibrationPoints;
    public float[] GetRelativeAngles() => relativePointAngles;
    public int GetNextIndex() => nextPointIndex;
    public BearingReferenceSymbol[] GetPointNumbers() => pointNumbers;
    
    public void ScheduledUpdate()
    {
        if (StoryGoalManager.main.IsGoalComplete("OnCalibrationRunCompleted")) return;
        
        if (doingCalibrationRun) return;
        
        if (!(Vector3.Distance(calibrationPoints[0], transform.position) < distToCountAsReached)) return;
        
        doingCalibrationRun = true;
        OnPointReached?.Invoke(0);
        calibrationObjects.SetActive(true);
        voiceNotificationManager.PlayVoiceNotification(startedCalibrationVoiceline);
        FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("DefenseDoorSignal_Searching"), transform.position);
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