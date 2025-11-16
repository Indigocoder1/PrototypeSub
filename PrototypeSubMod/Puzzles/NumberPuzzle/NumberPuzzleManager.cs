using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Puzzles.NumberPuzzle;

public class NumberPuzzleManager : MonoBehaviour
{
    public event Action onPuzzleCompleted;
    
    [SerializeField] private NumberPuzzleAnswer[] puzzleAnswers;
    [SerializeField] private Sprite[] numberSprites;
    [SerializeField] private SelectableNumber[] selectableNumbers;
    [SerializeField] private Color deselectedColor;
    [SerializeField] private Color selectedColor;

    private NumberPuzzleAnswer[] puzzleAnswersOrder;
    private NumberPuzzleAnswer prevSelectedAnswer;
    
    private int selectedIndex;
    private int primaryNumber = -1;
    private int secondaryNumber = -1;
    private int previousSum = -1;

    private void Start()
    {
        int index = 0;
        List<int> indices = new();
        puzzleAnswersOrder = new NumberPuzzleAnswer[puzzleAnswers.Length];
        foreach (var answer in puzzleAnswers)
        {
            answer.GetImage().color = deselectedColor;
            answer.SetSprite(numberSprites[index]);
            answer.onClicked += OnClickedAnswer;
            indices.Add(index);
            puzzleAnswersOrder[index] = answer;
            index++;
        }
        
        for (int i = 0; i < puzzleAnswersOrder.Length; i++)
        {
            int newIndex = indices[Random.Range(0, indices.Count - 1)];
            for (int j = 0; j < 5; j++)
            { 
                // Make sure the selected number is not the same as the current one
                // 5 iteration limit
                if (newIndex != i) break;
                newIndex = indices[Random.Range(0, indices.Count - 1)];
            }
            indices.Remove(newIndex);
            var item1 = puzzleAnswersOrder[i];
            var item2 = puzzleAnswersOrder[newIndex];
            puzzleAnswersOrder[i] = item2;
            puzzleAnswersOrder[newIndex] = item1;
            puzzleAnswersOrder[newIndex].SwapPosition(puzzleAnswersOrder[i]);
        }
    }

    private void OnClickedAnswer(NumberPuzzleAnswer answer)
    {
        if (prevSelectedAnswer == null)
        {
            prevSelectedAnswer = answer;
            return;
        }
        
        if (prevSelectedAnswer == answer) return;
        
        int index1 = Array.IndexOf(puzzleAnswersOrder, answer);
        int index2 = Array.IndexOf(puzzleAnswersOrder, prevSelectedAnswer);
        puzzleAnswersOrder[index1] = prevSelectedAnswer;
        puzzleAnswersOrder[index2] = answer;
        answer.SwapPosition(prevSelectedAnswer);
        prevSelectedAnswer = null;
    }

    public void SelectNumber(int number, bool? isSecondary = null)
    {
        if (secondaryNumber > 0)
        {
            selectableNumbers[secondaryNumber - 1].EnableSelectedIndicators(0);
        }

        if (primaryNumber > 0)
        {
            selectableNumbers[primaryNumber - 1].EnableSelectedIndicators(0);
        }

        if (isSecondary == null)
        {
            if (selectedIndex == 0)
            {
                if (number == primaryNumber)
                {
                    secondaryNumber = number;
                    selectedIndex++;
                }
                primaryNumber = number;
            }
            else
            {
                secondaryNumber = number;
            }
        }
        else
        {
            if (!isSecondary.Value)
            {
                primaryNumber = number;
            }
            else
            {
                secondaryNumber = number;
            }
        }
        
        if (primaryNumber > 0)
        {
            int addition = primaryNumber == secondaryNumber ? 1 : 0;
            selectableNumbers[primaryNumber - 1].EnableSelectedIndicators(1 + addition);
        }
        
        if (secondaryNumber > 0 && primaryNumber != secondaryNumber)
        {
            selectableNumbers[secondaryNumber - 1].EnableSelectedIndicators(1);
        }

        if (primaryNumber > 0 && secondaryNumber > 0)
        {
            RecalculateHint();
        }

        if (selectedIndex == 1 && secondaryNumber == number && primaryNumber == number) return;
        
        selectedIndex = (selectedIndex + 1) % 2;
    }

    private void RecalculateHint()
    {
        if (previousSum != -1)
        {
            puzzleAnswers[previousSum].GetImage().color = deselectedColor;
        }

        int sum = primaryNumber + secondaryNumber;
        puzzleAnswers[sum].GetImage().color = selectedColor;
        previousSum = sum;
    }

    public void OnConfirmationClicked()
    {
        if (!HasCorrectSequence())
        {
            ErrorMessage.AddError("Incorrect sequence");
            return;
        }
        
        ErrorMessage.AddError("Correct sequence entered!");
        onPuzzleCompleted?.Invoke();
    }

    private bool HasCorrectSequence()
    {
        int prevNumber = -1;
        foreach (var answer in puzzleAnswersOrder)
        {
            if (answer.GetNumber() != prevNumber + 1) return false;
            prevNumber++;
        }

        return true;
    }
}