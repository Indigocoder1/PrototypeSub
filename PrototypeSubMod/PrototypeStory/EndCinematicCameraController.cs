using PrototypeSubMod.Patches;
using System.Collections;
using PrototypeSubMod.Credits;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory;

internal class EndCinematicCameraController : MonoBehaviour
{
    [SerializeField] private EndingMusicManager musicManager;
    [SerializeField] private CyclopsExternalCams externalCams;
    [SerializeField] private ProtoRigidbodyFreezer freezeWhenFar;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private float cameraDelay;
    [SerializeField] private float creditsDelay;

    private void Start()
    {
        ProtoStoryLocker.onEndingStart += OnEndingStarts;
    }

    private void OnEndingStarts()
    {
        StartCoroutine(StartCameraDelay());
    }

    private IEnumerator StartCameraDelay()
    {
        yield return new WaitForSeconds(cameraDelay);

        ProtoScreenFadeManager.instance.FadeIn(2);
        yield return new WaitForSeconds(2);
        
        LockCameraPosition();
        Player.main.playerController.inputEnabled = false;
        Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);
        Player.main.GetPDA().Close();
        Player.main.GetPDA().SetIgnorePDAInput(true);
        
        ProtoScreenFadeManager.instance.FadeOut(2);

        yield return StartCreditsDelayed();
    }

    private IEnumerator StartCreditsDelayed()
    {
        yield return new WaitForSeconds(creditsDelay);

        ProtoScreenFadeManager.instance.FadeIn(1);
        musicManager.OnFadeToCredits();
        yield return new WaitForSeconds(1);
        
        CleanupScene();
    }

    private void CleanupScene()
    {
        FMODUnity.RuntimeManager.StopAllEvents(true);
        SceneCleaner_Patches.QueueSceneOverride();
        SceneCleaner.Open();
    }

    private void LockCameraPosition()
    {
        freezeWhenFar.enabled = false;

        externalCams.SetActive(false);
        Player.main.TryEject();
        MainCameraControl.main.enabled = false;
        SNCameraRoot.main.transform.position = cameraPos.position;
        SNCameraRoot.main.transform.rotation = cameraPos.rotation;
        SNCameraRoot.main.transform.SetParent(null);
        Player.main.groundMotor.enabled = false;

        GUIController_Patches.SetDenyHideCycling(true);
        GUIController.SetHidePhase(GUIController.HidePhase.All);

        VRUtil.Recenter();
    }

    private void OnDestroy()
    {
        GUIController_Patches.SetDenyHideCycling(false);
        ProtoStoryLocker.onEndingStart -= OnEndingStarts;
    }
}
