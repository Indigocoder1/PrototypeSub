using System;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmShoveSub : MonoBehaviour
{
    public event Action<SubRoot> OnHitSub;

    [SerializeField] private float shoveImpulse;
    [SerializeField] private FMOD_CustomEmitter impactSfx;

    private float timeLastHit;
    
    private void OnTriggerEnter(Collider other)
    {
        var subRoot = other.GetComponentInParent<SubRoot>();
        if (!subRoot) return;
        
        if (Time.time - timeLastHit < 0.1f) return;
        
        subRoot.rigidbody.AddForce(transform.forward * shoveImpulse, ForceMode.Impulse);
        impactSfx.Play();
        MainCameraControl.main.ShakeCamera(5);
        OnHitSub?.Invoke(subRoot);
        timeLastHit = Time.time;
    }
}