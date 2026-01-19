using PrototypeSubMod.Puzzles.BearingPuzzle;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationDirectionIndicator : MonoBehaviour
{
    [SerializeField] private Transform spriteParent;
    [SerializeField] private CalibrationRunManager runManager;
    [SerializeField] private BearingReferenceSymbol[] bearingReferenceSymbols;

    private void Start()
    {
        runManager.onPointReached += OnPointReached;
    }

    private void OnPointReached(int index)
    {
        if (index < 0 || index > bearingReferenceSymbols.Length - 1) return;

        foreach (Transform child in spriteParent)
        {
            Destroy(child.gameObject);
        }

        var symbolObject = bearingReferenceSymbols[index].CreateSymbolObject();
        symbolObject.transform.SetParent(spriteParent, false);
    }

    public BearingReferenceSymbol[] GetBearingReferenceSymbols() => bearingReferenceSymbols;
}