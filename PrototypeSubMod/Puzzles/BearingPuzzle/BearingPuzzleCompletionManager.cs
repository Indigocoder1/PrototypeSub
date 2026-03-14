using System.Collections;
using PrototypeSubMod.MiscMonobehaviors;
using PrototypeSubMod.Puzzles.NumberPuzzle;
using PrototypeSubMod.Registration;
using Story;
using UnityEngine;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingPuzzleCompletionManager : MonoBehaviour
{
    [SerializeField] private LightingController lightingController;
    [SerializeField] private SequencedLightEnabler sequencedLightEnabler;

    private void Start()
    {
        if (!StoryGoalManager.main.IsGoalComplete("ProtoBearingPuzzleComplete")) return;

        OnPuzzleCompleted();
        lightingController.SnapToState(2);
        sequencedLightEnabler.ActivateLightsSequentially();
    }

    private void OnPuzzleCompleted()
    {
        StoryGoalManager.main.OnGoalComplete("ProtoBearingPuzzleComplete");
    }

    public void OnFactorDownloaded()
    {
        UWE.CoroutineHost.StartCoroutine(DisableLightsDelayed());
    }

    private IEnumerator DisableLightsDelayed()
    {
        yield return new WaitForSeconds(6.6f);
        lightingController.LerpToState(2);
        sequencedLightEnabler.ActivateLightsSequentially();
    }
}