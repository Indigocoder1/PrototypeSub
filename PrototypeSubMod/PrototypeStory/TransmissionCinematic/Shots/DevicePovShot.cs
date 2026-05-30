using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class DevicePovShot : CinematicShot
{
    [SerializeField] private FMOD_CustomEmitter deviceSfx;
    
    public override event Action<DeviceCinematicManager> OnShotCompleted;

    public override void PlayShot(Animator animator, DeviceCinematicManager cinematicManager)
    {
        base.PlayShot(animator, cinematicManager);
        deviceSfx.Play();
    }

    public void EndPovShot()
    {
        OnShotCompleted?.Invoke(deviceCinematicManager);
    }
}