using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingSymbolIndicator : BearingSymbol
{
    [SerializeField] private BearingReferenceSymbol referenceSymbol;

    private GameObject symbol;

    private void Start()
    {
        RefreshSprite();
    }

    public override void RefreshSprite()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        symbol = referenceSymbol.CreateSymbolObject();
        symbol.transform.SetParent(transform, false);
    }

    public void SetReferenceSymbol(BearingReferenceSymbol symbol)
    {
        referenceSymbol = symbol;
        RefreshSprite();
    }

    public override BearingReferenceSymbol GetReferenceSymbol() => referenceSymbol;
}