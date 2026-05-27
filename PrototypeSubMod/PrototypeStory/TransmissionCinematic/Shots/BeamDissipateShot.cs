using System;
using PrototypeSubMod.MiscMonobehaviors;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class BeamDissipateShot : CinematicShot
{
    public override event Action<DeviceCinematicManager> OnShotCompleted;

    [SerializeField] private QepLaserSpawner laserSpawner;
    [SerializeField] private PostProcessOverride largeBloomOverride;
    [SerializeField] private BeamAlphaController alphaController;
    [SerializeField] private ParticleSystem laserParticles;

    public override void PlayShot(Animator animator, DeviceCinematicManager cinematicManager)
    {
        base.PlayShot(animator, cinematicManager);
        laserSpawner.PlayLaserVFX();
        largeBloomOverride.ApplyOverrides();
    }

    public void PlayAlphaEffect(float duration)
    {
        alphaController.PlayAlphaAnimation(duration);
    }

    public void PlayBeamParticles()
    {
        laserParticles.Play();
    }

    public void EndBeamDissipateShot()
    {
        OnShotCompleted?.Invoke(deviceCinematicManager);
    }
}