using System;
using PrototypeSubMod.IonGenerator;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Interceptor;

public class ReactorEMPManager : MonoBehaviour
{
    [SerializeField] private InterceptorReactorSequenceManager sequenceManager;
    [SerializeField] private EmpSpawner empSpawner;
    [SerializeField] private FMOD_CustomEmitter empSfx;
    [SerializeField] private float timeBetweenEMPs;

    private float empTimer;

    private void Update()
    {
        if (empTimer < timeBetweenEMPs)
        {
            empTimer += Time.deltaTime;
        }
        else
        {
            empSpawner.FireEMP(0, OnTouch);
            empSfx.Play();
            empTimer = 0;
        }
    }

    private void OnTouch(Collider collider)
    {
        if (collider != Player.mainCollider) return;
        
        sequenceManager.StartReactorSequence();
        Player.main.TryEject();
    }
}