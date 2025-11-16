using UnityEngine;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class BearingReferenceSymbol : MonoBehaviour
{
    [SerializeField] private Sprite sprite;

    public Sprite GetSprite() => sprite;
}