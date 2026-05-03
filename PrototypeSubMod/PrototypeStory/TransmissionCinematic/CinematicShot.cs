using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic;

public abstract class CinematicShot : MonoBehaviour
{
    public abstract event Action OnShotCompleted;
    
    [SerializeField] private string animatorTrigger;

    public virtual void PlayShot(Animator animator)
    {
        animator.SetTrigger(animatorTrigger);
    }
}