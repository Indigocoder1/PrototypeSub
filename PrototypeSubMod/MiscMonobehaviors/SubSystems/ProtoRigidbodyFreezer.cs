using System;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class ProtoRigidbodyFreezer : MonoBehaviour, IProtoTreeEventListener
{
    [SerializeField] private float freezeDistance = 48f;
    [SerializeField] private Transform[] colliderActivationStages;
    
    private Rigidbody rigidbody;
    private Coroutine updateCoroutine;
    private bool collidersTransitioning;
    private bool wasInDistance;
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        bool inDistance = (MainCamera.camera.transform.position - transform.position).sqrMagnitude < freezeDistance * freezeDistance;
        if (inDistance != wasInDistance)
        {
            if (collidersTransitioning && updateCoroutine != null)
            {
                UWE.CoroutineHost.StopCoroutine(updateCoroutine);
            }
            
            updateCoroutine = UWE.CoroutineHost.StartCoroutine(UpdateCollidersAndKinematics(inDistance));
        }

        wasInDistance = inDistance;
    }

    private IEnumerator UpdateCollidersAndKinematics(bool inDistance, bool skipWait = false)
    {
        collidersTransitioning = true;
        
        if (inDistance)
        {
            UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, false);
        }
        
        for (int i = 0; i < colliderActivationStages.Length; i++)
        {
            colliderActivationStages[i].gameObject.SetActive(inDistance);
            if (!skipWait)
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
            }
        }
        
        if (!inDistance)
        {
            UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, true);
        }
        
        collidersTransitioning = false;
    }

    public void OnProtoSerializeObjectTree(ProtobufSerializer serializer) { }

    public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
    {
        rigidbody = GetComponent<Rigidbody>();
        bool inDistance = (MainCamera.camera.transform.position - transform.position).sqrMagnitude < freezeDistance * freezeDistance;
        UWE.CoroutineHost.StartCoroutine(UpdateCollidersAndKinematics(inDistance, true));
    }
}