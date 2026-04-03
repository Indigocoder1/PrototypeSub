using Nautilus.Utility;
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
            onRoomComplete?.Invoke();
            roomCompleted = true;
        }
        else
        {
            FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("TetherFactorNoPower"), Player.main.transform.position);
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