using System;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Puzzles.NumberPuzzle;

public class SelectableNumber : MonoBehaviour
{
    [SerializeField] private NumberPuzzleManager puzzleManager;
    [SerializeField] private Image[] selectedIndicators;
    [SerializeField] private int representativeNumber;

    private void Start()
    {
        EnableSelectedIndicators(0);
    }

    public void Select()
    {
        puzzleManager.SelectNumber(representativeNumber);
    }

    public void EnableSelectedIndicators(int amount)
    {
        if (amount > selectedIndicators.Length)
        {
            throw new System.Exception($"Tried to enable more selected indicators than assigned! ({amount})");
        }

        int index = 0;
        foreach (var indicator in selectedIndicators)
        {
            indicator.gameObject.SetActive(index < amount);
            index++;
        }
    }
}