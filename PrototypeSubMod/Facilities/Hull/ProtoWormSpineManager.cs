using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull;

public class ProtoWormSpineManager : MonoBehaviour
{
    [SerializeField] private Transform segmentsParent;
    [SerializeField] private GameObject spineSegmentPrefab;
    [SerializeField] private Vector3 initialLocalPos;
    [SerializeField] private Vector3 incrementPerSpine;
    [SerializeField] private int spineSegmentCount;

    private bool spawned;
    
    private void Start()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        for (int i = 0; i < spineSegmentCount; i++)
        {
            var spine = Instantiate(spineSegmentPrefab, segmentsParent);
            spine.transform.localPosition = initialLocalPos + incrementPerSpine * i;
        }

        spawned = true;
    }

    public bool GetSpawned() => spawned;
    public Vector3 GetInitialLocalPos() => initialLocalPos;
    public Vector3 GetIncrementPerSpine() => incrementPerSpine;
    public int GetSpineSegmentCount() => spineSegmentCount;
    public Transform GetChild(int index) => segmentsParent.GetChild(index);
}