using System;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Puzzles.NumberPuzzle;

public class NumberPuzzleAnswer : MonoBehaviour
{
    public event Action<NumberPuzzleAnswer> onClicked;

    [SerializeField] private int representativeNumber;
    [SerializeField] private Image image;

    public void SwapPosition(NumberPuzzleAnswer swapWith)
    {
        (transform.position, swapWith.transform.position) = (swapWith.transform.position, transform.position);
    }

    public Image GetImage() => image;
    public int GetNumber() => representativeNumber;

    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void OnClicked()
    {
        onClicked?.Invoke(this);
    }

    public override string ToString()
    {
        return base.ToString() + $"_{representativeNumber}";
    }
}