using UnityEngine;

namespace PrototypeSubMod.Teleporter;

public class MapPositionIndicator : MonoBehaviour, IScheduledUpdateBehaviour
{
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private Transform[] subIndicators;
    [SerializeField] private float positionScaleFactor;
    [SerializeField] private float maxDistanceFromCenter;

    public string GetProfileTag() => "MapPositionIndicator";

    public void ScheduledUpdate()
    {
        Vector3 scaledPos = subRoot.transform.position * positionScaleFactor;
        var tooFarFromCenter = Vector2.Distance(Vector2.zero, new Vector2(scaledPos.x, scaledPos.z)) > maxDistanceFromCenter;
        foreach (var indicator in subIndicators)
        {
            indicator.gameObject.SetActive(!tooFarFromCenter);
            if (tooFarFromCenter) continue;
            
            indicator.localPosition = new Vector3(scaledPos.x, scaledPos.z, indicator.localPosition.z);
            indicator.localEulerAngles = new Vector3(0, 0, 360 - subRoot.transform.localEulerAngles.y);
        }
    }

    public int scheduledUpdateIndex { get; set; }

    private void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }
    
    private void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }
}