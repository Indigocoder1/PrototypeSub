using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingSymbolIndicator : MonoBehaviour
{
    [SerializeField] private BearingReferenceSymbol referenceSymbol;
    [SerializeField] private Image image;

    private void Start()
    {
        RefreshSprite();
    }

    public void RefreshSprite()
    {
        image.sprite = referenceSymbol.GetSprite();
    }
}