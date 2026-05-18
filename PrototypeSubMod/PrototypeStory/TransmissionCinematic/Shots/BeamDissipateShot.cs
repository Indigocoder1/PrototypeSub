using System;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class BeamDissipateShot : CinematicShot
{
    public override event Action<DeviceCinematicManager> OnShotCompleted;

    [SerializeField] private QepLaserSpawner laserSpawner;

    public override void PlayShot(Animator animator, DeviceCinematicManager cinematicManager)
    {
        base.PlayShot(animator, cinematicManager);
        laserSpawner.PlayLaserVFX();
    }
}