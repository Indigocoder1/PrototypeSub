using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationProgressTracker : MonoBehaviour, IScheduledUpdateBehaviour
{
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private CalibrationRunManager runManager;
    [SerializeField] private Transform subIcon;
    [SerializeField] private float positionScalar;
    
    public string GetProfileTag() => "CalibrationRunProgressTracker";
    public int scheduledUpdateIndex { get; set; }

    public void ScheduledUpdate()
    {
        var localPos = subRoot.transform.position - runManager.GetSiteCenter();
        localPos /= positionScalar;

        subIcon.transform.localPosition = new Vector3(localPos.x, localPos.z);
    }

    public  void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }

    public void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }
}