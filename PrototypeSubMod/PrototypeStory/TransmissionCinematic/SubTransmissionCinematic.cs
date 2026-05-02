using System;
using System.Collections;
using System.Collections.Generic;
using PrototypeSubMod.Credits;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.PrototypeStory.TransmissionDevice;
using UnityEngine;
using UnityEngine.Serialization;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic;

public class SubTransmissionCinematic : MonoBehaviour
{
    private static readonly int TransmissionDeactivated = Animator.StringToHash("TransmissionDeactivated");
    public event Action OnCinematicComplete;

    [SerializeField] private EmissionColorController subEmissionController;
    [SerializeField] private EmissionColorController finEmissionController;
    [SerializeField] private ProtoFinsManager finsManager;
    [SerializeField] private Transform transmissionDeviceLocation;
    
    [Header("Animators")]
    [SerializeField] private Animator pistonsAnimator;
    [SerializeField] private Animator cinematicAnimator;
    
    public void PlayCinematic()
    {
        PlayCinematic(null);
    }
    
    public void PlayCinematic(TransmissionDeviceManager transmissionDeviceManager)
    {
        StartCoroutine(PlayCinematicAsync(transmissionDeviceManager));
    }

    private IEnumerator PlayCinematicAsync(TransmissionDeviceManager transmissionDeviceManager)
    {
        const float fadeTime = 0.2f;
        ProtoScreenFadeManager.instance.FadeIn(fadeTime);
        yield return new WaitForSeconds(fadeTime);

        subEmissionController.enabled = false;
        
        if (transmissionDeviceManager)
        {
            transmissionDeviceManager.transform.position = transmissionDeviceLocation.position;
            transmissionDeviceManager.transform.rotation = transmissionDeviceLocation.rotation;
        }
        
        cinematicAnimator.SetTrigger("StartCinematic");
        
        ProtoScreenFadeManager.instance.FadeOut(fadeTime);
    }

    public void DeactivateFins()
    {
        finsManager.SetTransmissionDeactivated(true);
    }

    public void DeactivatePistons()
    {
        pistonsAnimator.SetBool(TransmissionDeactivated, true);
        finEmissionController.RegisterTempColor(this, new EmissionColorController.EmissionRegistrarData(Color.black));
    }
    
    public void EndCinematic()
    {
        finsManager.SetTransmissionDeactivated(false);
        pistonsAnimator.SetBool(TransmissionDeactivated, false);
        finEmissionController.RemoveTempColor(this);
        subEmissionController.enabled = true;
        subEmissionController.ForceUpdate();
    }

    private void OnDisable()
    {
        EndCinematic();
    }
}