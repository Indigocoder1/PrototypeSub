using UnityEngine;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public abstract class BearingSymbol : MonoBehaviour
{
    public abstract BearingReferenceSymbol GetReferenceSymbol();
    public abstract void RefreshSprite();
}