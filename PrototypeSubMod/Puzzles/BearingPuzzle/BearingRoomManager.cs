using System;
using UnityEngine;
using UnityEngine.Events;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingRoomManager : MonoBehaviour
{
    [SerializeField] private CyclingBearingIndicator[] cyclingIndicators;
    [SerializeField] private UnityEvent onRoomComplete;
    [SerializeField] private FMODAsset correctSequenceSfx;
    [SerializeField] private FMODAsset incorrectSequenceSfx;
    [SerializeField] private float sfxVolume = 1;

    private bool roomCompleted;
    
    public void OnConfirmationClicked()
    {
        if (roomCompleted) return;
        
        if (HasCorrectSequence())
        {
            ErrorMessage.AddError(Language.main.Get("ProtoPuzzleCorrectSequence"));
            onRoomComplete?.Invoke();
            FMODUWE.PlayOneShot(correctSequenceSfx, Player.main.transform.position, sfxVolume);
            roomCompleted = true;
        }
        else
        {
            ErrorMessage.AddError(Language.main.Get("ProtoPuzzleIncorrectSequence"));
            FMODUWE.PlayOneShot(incorrectSequenceSfx, Player.main.transform.position, sfxVolume);
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