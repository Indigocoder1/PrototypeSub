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

    public override BearingReferenceSymbol GetReferenceSymbol() => copySymbol.GetReferenceSymbol();

    public override void RefreshSprite()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        var symbol = copySymbol.GetReferenceSymbol().CreateSymbolObject();
        symbol.transform.SetParent(transform, false);
    }
}