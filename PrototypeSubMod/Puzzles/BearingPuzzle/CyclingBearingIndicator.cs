using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class CyclingBearingIndicator : MonoBehaviour
{
    [SerializeField] private BearingReferenceSymbol[] availableReferenceSymbols;
    [SerializeField] private BearingReferenceSymbol correctSymbol;
    [SerializeField] private Image image;
    
    private int currentIndex;

    private void Start()
    {
        currentIndex = Random.Range(0, availableReferenceSymbols.Length - 1);

        for (int i = 0; i < 4; i++)
        {
            if (GetCurrentSymbol() != correctSymbol) break;
            
            currentIndex = Random.Range(0, availableReferenceSymbols.Length - 1);
        }
        
        image.sprite = GetCurrentSymbol().GetSprite();
    }

    public void CycleIndicator()
    {
        currentIndex++;
        currentIndex %= availableReferenceSymbols.Length;
        image.sprite = GetCurrentSymbol().GetSprite();
    }

    private BearingReferenceSymbol GetCurrentSymbol()
    {
        return availableReferenceSymbols[currentIndex];
    }

    public bool OnCorrectSymbol()
    {
        return GetCurrentSymbol() == correctSymbol;
    }
}