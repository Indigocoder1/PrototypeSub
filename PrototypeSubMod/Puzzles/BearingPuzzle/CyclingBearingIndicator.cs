using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class CyclingBearingIndicator : MonoBehaviour
{
    [SerializeField] private BearingReferenceSymbol[] availableReferenceSymbols;
    [SerializeField] private BearingSymbol correctSymbol;
    [SerializeField] private Button button;

    private GameObject symbolObject;
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
        if (symbolObject != null)
        {
            Destroy(symbolObject);
        }

        symbolObject = GetCurrentSymbol().CreateSymbolObject();
        symbolObject.transform.SetParent(transform, false);
        var symbolImage = symbolObject.transform.Find("Image").GetComponent<Image>();
        button.targetGraphic = symbolImage;
    }

    private BearingReferenceSymbol GetCurrentSymbol()
    {
        return availableReferenceSymbols[currentIndex];
    }

    public bool OnCorrectSymbol()
    {
        return GetCurrentSymbol() == correctSymbol.GetReferenceSymbol();
    }
}