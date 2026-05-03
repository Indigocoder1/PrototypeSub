using System;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class FinDisableShot : CinematicShot
{
    private static readonly int TransmissionDeactivated = Animator.StringToHash("TransmissionDeactivated");
    
    public override event Action OnShotCompleted;
    
    [SerializeField] private EmissionColorController finEmissionController;
    [SerializeField] private ProtoFinsManager finsManager;
    [SerializeField] private Animator pistonsAnimator;

    public void DeactivateFins()
    {
        finsManager.SetTransmissionDeactivated(true);
    }

    public void DeactivatePistons()
    {
        pistonsAnimator.SetBool(TransmissionDeactivated, true);
        finEmissionController.RegisterTempColor(this, new EmissionColorController.EmissionRegistrarData(Color.black));
    }

    public void EndFinsShot()
    {
        OnShotCompleted?.Invoke();
    }
}