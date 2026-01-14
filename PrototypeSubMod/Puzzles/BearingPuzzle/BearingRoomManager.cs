using System;
using UnityEngine;
using UnityEngine.Events;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingRoomManager : MonoBehaviour
{
    [SerializeField] private CyclingBearingIndicator[] cyclingIndicators;
    [SerializeField] private UnityEvent onRoomComplete;

    public void OnConfirmationClicked()
    {
        if (HasCorrectSequence())
        {
            ErrorMessage.AddError(Language.main.Get("ProtoPuzzleCorrectSequence"));
            onRoomComplete?.Invoke();
        }
        else
        {
            ErrorMessage.AddError(Language.main.Get("ProtoPuzzleIncorrectSequence"));
        }
    }

    private bool HasCorrectSequence()
    {
        foreach (var indicator in cyclingIndicators)
        {
            if (!indicator.OnCorrectSymbol()) return false;
        }

        return true;
    }
}