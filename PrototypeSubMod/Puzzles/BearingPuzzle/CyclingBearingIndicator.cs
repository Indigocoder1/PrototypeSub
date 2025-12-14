using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class CyclingBearingIndicator : MonoBehaviour
{
    [SerializeField] private BearingReferenceSymbol[] availableReferenceSymbols;
    [SerializeField] private BearingSymbol correctSymbol;
    
    private int currentIndex;

    private void Start()
    {
        currentIndex = Random.Range(0, availableReferenceSymbols.Length - 1);

        for (int i = 0; i < 4; i++)
        {
            if (GetCurrentSymbol() != correctSymbol.GetReferenceSymbol()) break;
            
            currentIndex = Random.Range(0, availableReferenceSymbols.Length - 1);
        }

        RefreshSprite();
    }

    public void CycleIndicator()
    {
        currentIndex++;
        currentIndex %= availableReferenceSymbols.Length;
        RefreshSprite();
    }

    private void RefreshSprite()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        var symbol = GetCurrentSymbol().CreateSymbolObject();
        symbol.transform.SetParent(transform, false);
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