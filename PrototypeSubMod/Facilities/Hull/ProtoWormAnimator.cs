using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull;

public abstract class ProtoWormAnimator : MonoBehaviour
{
    private static readonly int StartMoving = Animator.StringToHash("StartMoving");
    
    [SerializeField] protected ProtoWormSpineManager spineManager;
    [SerializeField] protected Transform spineSegmentsParent;

    private readonly Dictionary<Transform, Animator> segmentAnimators = new();
    protected readonly List<FollowPoint> followPoints = new();
    protected float absIncrement;
    
    protected virtual void Start()
    {
        absIncrement = Mathf.Abs(spineManager.GetIncrementPerSpine().z);
        UWE.CoroutineHost.StartCoroutine(InitializeSegmentAnimators());
    }

    private IEnumerator InitializeSegmentAnimators()
    {
        yield return new WaitUntil(() => spineManager.GetSpawned());

        foreach (Transform child in spineSegmentsParent)
        {
            segmentAnimators[child] = child.GetComponentInChildren<Animator>(true);
        }
    }

    protected virtual void Update()
    {
        UpdateFollowPoints();
        
        float progress = (GetDistanceMoved() % absIncrement) / absIncrement;
        for (int i = 0; i < spineSegmentsParent.childCount; i++)
        {
            if (followPoints.Count == 0) break;
            
            var child = spineSegmentsParent.GetChild(i);

            if (!GetShouldUpdateSpineSegment(child, i))
            {
                continue;
            }
            
            if (!child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
                segmentAnimators[child].SetTrigger(StartMoving);
            }

            UpdateSpineSegment(child, i, progress);
        }
    }

    protected abstract void UpdateFollowPoints();
    public abstract float GetDistanceMoved();
    
    protected virtual bool GetShouldUpdateSpineSegment(Transform child, int index)
    {
        if (index >= followPoints.Count - 1)
        {
            child.gameObject.SetActive(false);
            child.position = followPoints[0].position;
            return false;
        }

        return true;
    }

    private void UpdateSpineSegment(Transform child, int index, float progress)
    {
        int invertedIndex = followPoints.Count - index - 1;
        var prevPoint = followPoints[invertedIndex - 1];
        var targetPoint = followPoints[invertedIndex];
        child.position = Vector3.Lerp(targetPoint.position, prevPoint.position, 1 - progress);
        
        child.rotation = Quaternion.Lerp(prevPoint.rotation, targetPoint.rotation, progress);
    }
    
    protected class FollowPoint
    {
        public Vector3 position;
        public Quaternion rotation;

        public FollowPoint(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}