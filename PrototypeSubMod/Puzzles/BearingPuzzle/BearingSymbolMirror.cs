using System;
using UnityEngine;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingSymbolMirror : BearingSymbol
{
    [SerializeField] private BearingSymbol copySymbol;

    private void OnValidate()
    {
        if (copySymbol == this)
        {
            copySymbol = null;
        }
    }

    private void Start()
    {
        RefreshSprite();
    }

    public override BearingReferenceSymbol GetReferenceSymbol() => copySymbol.GetReferenceSymbol();

    public override void RefreshSprite()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        var symbol = copySymbol.GetReferenceSymbol().CreateSymbolObject();
        symbol.transform.SetParent(transform, false);
    }
}