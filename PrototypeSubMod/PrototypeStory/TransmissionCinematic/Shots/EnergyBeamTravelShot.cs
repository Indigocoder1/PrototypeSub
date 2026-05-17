using System;
using PrototypeSubMod.MiscMonobehaviors;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class EnergyBeamShot : CinematicShot
{
    public override event Action<DeviceCinematicManager> OnShotCompleted;

    [SerializeField] private PostProcessOverride defaultPostProcessing;

    public override void PlayShot(Animator animator, DeviceCinematicManager cinematicManager)
    {
        base.PlayShot(animator, cinematicManager);
        defaultPostProcessing.ApplyOverrides();
    }

    public void EndEnergyBeamShot()
    {
        OnShotCompleted?.Invoke(deviceCinematicManager);
    }
}