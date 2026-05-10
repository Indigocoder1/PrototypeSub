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
    
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private Transform transmissionDeviceLocation;
    [SerializeField] private Animator cinematicAnimator;
    [SerializeField] private EmissionColorController subEmissionController;
    [SerializeField] private PrecursorTeleporter subTeleporter;
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
    
    public void PlayCinematic(DeviceCinematicManager cinematicManager)
    {
        materialSwapper.SwapMaterials();
        sdfCutout.SetActive(false);
        functionalityRoot.SetActive(true);
        cinematicAnimator.enabled = true;
        subRoot.enabled = false;
        subRoot.lightControl.emissiveController.renderers.Clear();
        subRoot.lightControl.LerpToState(2);
        subTeleporter.ToggleDoor(false);
        StartCoroutine(PlayCinematicAsync(cinematicManager));
    }

    private IEnumerator PlayCinematicAsync(DeviceCinematicManager cinematicManager)
    {
        const float fadeTime = 0.3f;
        ProtoScreenFadeManager.instance.FadeIn(fadeTime);
        yield return new WaitForSeconds(fadeTime);

        var shot = cinematicShots[shotIndex];
        shot.PlayShot(cinematicAnimator, cinematicManager);
        shot.OnShotCompleted += OnShotCompleted;
        
        subEmissionController.enabled = false;
        
        subRoot.transform.position = Plugin.TransmissionSitePos;
        
        if (cinematicManager)
        {
            cinematicManager.transform.position = transmissionDeviceLocation.position;
            cinematicManager.transform.rotation = transmissionDeviceLocation.rotation;
        }
        
        ProtoScreenFadeManager.instance.FadeOut(fadeTime);
    }

    private void OnShotCompleted(DeviceCinematicManager cinematicManager)
    {
        cinematicShots[shotIndex].OnShotCompleted -= OnShotCompleted;
        shotIndex++;
        
        if (shotIndex >= cinematicShots.Length) return;
        
        var shot = cinematicShots[shotIndex];
        shot.PlayShot(cinematicAnimator, cinematicManager);
        shot.OnShotCompleted += OnShotCompleted;
    }
}