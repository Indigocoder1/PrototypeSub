using System;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class FinDisableShot : CinematicShot
{
    private static readonly int TransmissionDeactivated = Animator.StringToHash("TransmissionDeactivated");
    
    public override event Action<DeviceCinematicManager> OnShotCompleted;
    
    [SerializeField] private EmissionColorController finEmissionController;
    [SerializeField] private ProtoFinsManager finsManager;
    [SerializeField] private Animator pistonsAnimator;
    
    [Header("SFX")]
    [SerializeField] private FMOD_CustomEmitter finsLockingSfx;

    public void DeactivateFins()
    {
        finsManager.SetTransmissionDeactivated(true);
        finsLockingSfx.Play();
    }

    public void DeactivatePistons()
    {
        pistonsAnimator.SetBool(TransmissionDeactivated, true);
        finEmissionController.RegisterTempColor(this, new EmissionColorController.EmissionRegistrarData(Color.black));
    }

    public void EndFinsShot()
    {
        OnShotCompleted?.Invoke(deviceCinematicManager);
    }
}