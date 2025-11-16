using UnityEngine;

namespace PrototypeSubMod.Puzzles.NumberPuzzle;

public class NumberPuzzleCompletionManager : MonoBehaviour
{
    [SerializeField] private NumberPuzzleManager puzzleManager;
    [SerializeField] private Animator doorAnimator;

    private void Start()
    {
        puzzleManager.onPuzzleCompleted += OnPuzzleCompleted;
    }

    private void OnPuzzleCompleted()
    {
        doorAnimator.SetBool("DoorOpen", true);
    }
}