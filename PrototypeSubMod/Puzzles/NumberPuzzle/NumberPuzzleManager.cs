using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Puzzles.NumberPuzzle;

public class NumberPuzzleManager : MonoBehaviour
{
    [SerializeField] private Image[] displayNumbers;
    [SerializeField] private SelectableNumber[] selectableNumbers;
    [SerializeField] private Color deselectedColor;
    [SerializeField] private Color selectedColor;

    private int selectedIndex;
    private int primaryNumber = -1;
    private int secondaryNumber = -1;
    private int previousSum = -1;

    private void Start()
    {
        foreach (var number in displayNumbers)
        {
            number.color = deselectedColor;
        }
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
            displayNumbers[previousSum - 1].color = deselectedColor;
        }

        int sum = primaryNumber + secondaryNumber;
        displayNumbers[sum - 1].color = selectedColor;
        previousSum = sum;
    }
}