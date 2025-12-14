using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class RandomBearingSymbol : BearingSymbol
{
    [SerializeField] private BearingReferenceSymbol[] possibleSymbols;

    private BearingReferenceSymbol selectedSymbol;
    private GameObject symbolObject;
    
    private void Start()
    {
        selectedSymbol = possibleSymbols[Random.Range(0, possibleSymbols.Length - 1)];
        RefreshSprite();
    }
    
    public override void RefreshSprite()
    {
        if (selectedSymbol == null)
        {
            selectedSymbol = possibleSymbols[Random.Range(0, possibleSymbols.Length - 1)];
        }
        
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        symbolObject = selectedSymbol.CreateSymbolObject();
        symbolObject.transform.SetParent(transform, false);
    }

    public override BearingReferenceSymbol GetReferenceSymbol() => selectedSymbol;
}