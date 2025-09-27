using System;
using UnityEngine;

namespace PrototypeSubMod.StasisPulse;

public class StasisPulseTrigger : MonoBehaviour
{
    [SerializeField] private ProtoStasisPulse stasisPulse;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        
        stasisPulse.OnHitObject(other);
    }
}