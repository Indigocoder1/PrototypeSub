using System;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Puzzles.NumberPuzzle;

public class SelectableNumber : MonoBehaviour
{
    [SerializeField] private NumberPuzzleManager puzzleManager;
    [SerializeField] private int representativeNumber;
    [SerializeField] private Renderer[] selectedIndicators;
    [SerializeField] private Color selectedEmissionColor;
    [SerializeField] private Color deselectedEmissionColor;
    
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
            throw new Exception($"Tried to enable more selected indicators than assigned! ({amount})");
        }

        int index = 0;
        foreach (var indicator in selectedIndicators)
        {
            bool active = index < amount;
            var materials = indicator.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetColor(ShaderPropertyID._EmissionColor,
                    active ? selectedEmissionColor : deselectedEmissionColor);
            }

            indicator.materials = materials;
            index++;
        }
    }
}