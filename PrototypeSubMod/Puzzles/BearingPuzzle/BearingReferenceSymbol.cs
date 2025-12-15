using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

[CreateAssetMenu(fileName = "Bearing Symbol", menuName = "Prototype Sub/Bearing Symbol")]
public class BearingReferenceSymbol : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Color color = Color.white;
    [Range(-1, 1)]
    [SerializeField] private int maskSide;

    public Sprite GetSprite() => sprite;

    public GameObject CreateSymbolObject(Color? overrideColor = null)
    {
        var mask = new GameObject(name);
        var maskRect = mask.AddComponent<RectTransform>();
        var size = new Vector2(100 - 50 * Mathf.Abs(maskSide), 100);
        maskRect.sizeDelta = size;
        maskRect.localPosition += new Vector3(25, 0, 0) * maskSide;
        mask.gameObject.AddComponent<Image>().raycastTarget = false;
        var maskComponent = mask.gameObject.AddComponent<Mask>();
        maskComponent.showMaskGraphic = false;

        var spriteObject = new GameObject("Image");
        var spriteRect = spriteObject.AddComponent<RectTransform>();
        var spriteImage = spriteObject.AddComponent<Image>();
        spriteImage.sprite = sprite;
        spriteImage.color = overrideColor ?? color;
        spriteImage.raycastTarget = false;

        spriteRect.sizeDelta = new Vector2(100, 100);
        spriteObject.transform.SetParent(mask.transform);
        return mask;
    }
}