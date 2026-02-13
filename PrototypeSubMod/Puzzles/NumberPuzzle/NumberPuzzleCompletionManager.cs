using System.Collections;
using PrototypeSubMod.MiscMonobehaviors;
using Story;
using UnityEngine;

namespace PrototypeSubMod.Puzzles.NumberPuzzle;

public class NumberPuzzleCompletionManager : MonoBehaviour
{
    [SerializeField] private NumberPuzzleManager puzzleManager;
    [SerializeField] private LightingController lightingController;
    [SerializeField] private SequencedLightEnabler sequencedLightEnabler;
    [SerializeField] private Animator doorAnimator;

    private void Start()
    {
        puzzleManager.onPuzzleCompleted += OnPuzzleCompleted;
        
        if (!StoryGoalManager.main.IsGoalComplete("ProtoNumberPuzzleComplete")) return;
        
        OnPuzzleCompleted();
        lightingController.SnapToState(2);
        sequencedLightEnabler.ActivateLightsSequentially();
    }

    private void OnPuzzleCompleted()
    {
        doorAnimator.SetBool("DoorOpen", true);
        PDAEncyclopedia.Add("ProtoNumbersEncy", true);
    }

    public void OnFactorDownloaded()
    {
        StartCoroutine(DisableLightsDelayed());
    }

    private IEnumerator DisableLightsDelayed()
    {
        yield return new WaitForSeconds(6.6f);
        lightingController.LerpToState(2);
        sequencedLightEnabler.ActivateLightsSequentially();
    }
}