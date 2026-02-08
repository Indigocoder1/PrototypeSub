using Story;
using UnityEngine;

namespace PrototypeSubMod.Puzzles.NumberPuzzle;

public class NumberPuzzleCompletionManager : MonoBehaviour
{
    [SerializeField] private NumberPuzzleManager puzzleManager;
    [SerializeField] private LightingController lightingController;
    [SerializeField] private Animator doorAnimator;

    private void Start()
    {
        puzzleManager.onPuzzleCompleted += OnPuzzleCompleted;
        if (StoryGoalManager.main.IsGoalComplete("ProtoNumberPuzzleComplete"))
        {
            OnPuzzleCompleted();
            lightingController.SnapToState(1);
        }
    }

    private void OnPuzzleCompleted()
    {
        doorAnimator.SetBool("DoorOpen", true);
    }

    public void OnFactorDownloaded()
    {
        lightingController.LerpToState(1);
    }
}