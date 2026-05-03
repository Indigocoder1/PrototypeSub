using System;
using System.Collections;
using PrototypeSubMod.Credits;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using PrototypeSubMod.PrototypeStory.TransmissionDevice;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic;

public class SubTransmissionCinematic : MonoBehaviour
{
    public event Action OnCinematicComplete;
    
    [SerializeField] private Transform transmissionDeviceLocation;
    [SerializeField] private Animator cinematicAnimator;
    [SerializeField] private EmissionColorController subEmissionController;
    [SerializeField] private GameObject sdfCutout;
    [SerializeField] private GameObject functionalityRoot;
    [SerializeField] private MaterialSwapper materialSwapper;
    [SerializeField] private CinematicShot[] cinematicShots;

    private int shotIndex;

    private void Start()
    {
        cinematicAnimator.enabled = false;
        functionalityRoot.SetActive(false);
    }

    public void PlayCinematic()
    {
        PlayCinematic(null);
    }
    
    public void PlayCinematic(TransmissionDeviceManager transmissionDeviceManager)
    {
        materialSwapper.SwapMaterials();
        sdfCutout.SetActive(false);
        functionalityRoot.SetActive(true);
        cinematicAnimator.enabled = true;
        StartCoroutine(PlayCinematicAsync(transmissionDeviceManager));
    }

    private IEnumerator PlayCinematicAsync(TransmissionDeviceManager transmissionDeviceManager)
    {
        const float fadeTime = 0.3f;
        ProtoScreenFadeManager.instance.FadeIn(fadeTime);
        yield return new WaitForSeconds(fadeTime);

        var shot = cinematicShots[shotIndex];
        shot.PlayShot(cinematicAnimator);
        shot.OnShotCompleted += OnShotCompleted;
        
        subEmissionController.enabled = false;
        
        if (transmissionDeviceManager)
        {
            transmissionDeviceManager.transform.position = transmissionDeviceLocation.position;
            transmissionDeviceManager.transform.rotation = transmissionDeviceLocation.rotation;
        }
        
        ProtoScreenFadeManager.instance.FadeOut(fadeTime);
    }

    private void OnShotCompleted()
    {
        cinematicShots[shotIndex].OnShotCompleted -= OnShotCompleted;
        shotIndex++;
        
        if (shotIndex >= cinematicShots.Length) return;
        
        var shot = cinematicShots[shotIndex];
        shot.PlayShot(cinematicAnimator);
        shot.OnShotCompleted += OnShotCompleted;
    }
}