using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic;

public abstract class CinematicShot : MonoBehaviour
{
    public abstract event Action<DeviceCinematicManager> OnShotCompleted;
    
    [SerializeField] private string animatorTrigger;

    protected DeviceCinematicManager deviceCinematicManager;
    
    public virtual void PlayShot(Animator animator, DeviceCinematicManager cinematicManager)
    {
        animator.SetTrigger(animatorTrigger);
        deviceCinematicManager = cinematicManager;
    }
}