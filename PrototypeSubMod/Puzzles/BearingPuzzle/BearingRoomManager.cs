using UnityEngine;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingRoomManager : MonoBehaviour
{
    [SerializeField] private CyclingBearingIndicator[] cyclingIndicators;

    public void OnConfirmationClicked()
    {
        if (HasCorrectSequence())
        {
            ErrorMessage.AddError(Language.main.Get("ProtoPuzzleCorrectSequence"));
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