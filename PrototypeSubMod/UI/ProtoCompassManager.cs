using System;
using SubLibrary.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.UI;

public class ProtoCompassManager : MonoBehaviour, IUIElement
{
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private Transform noMaskParent;
    [SerializeField] private Image compassImage;
    [SerializeField] private Image backgroundColumn;
    [SerializeField] private Image bearingMask;
    [SerializeField] private Image columnMask;
    [SerializeField] private Sprite rightMaskSprite;
    [SerializeField] private Sprite leftMaskSprite;
    [SerializeField] private Sprite fullMaskSprite;
    [SerializeField] private Sprite[] cardinalSprites;

    private Color _initialColor;

    private bool _isHighlightingAngle;
    private bool _updatedImageColors;
    private float _highlightedAngle;
    private Color _highlightedColor;
    
    private void Start()
    {
        bool onLeft = subRoot.transform.eulerAngles.y > 180;
        bearingMask.sprite = onLeft ? leftMaskSprite : rightMaskSprite;
        columnMask.sprite = onLeft ? rightMaskSprite : leftMaskSprite;
        columnMask.gameObject.SetActive(true);
        _initialColor = compassImage.color;
    }

    public void UpdateUI()
    {
        var angle = subRoot.transform.eulerAngles.y;
        int index;
        bool onLeft;
        if (angle < 180)
        {
            onLeft = false;
            index = Mathf.RoundToInt(angle / 180f * 8f);
        }
        else
        {
            onLeft = true;
            index = Mathf.RoundToInt((360 - angle) / 180f * 8f);
        }

        compassImage.sprite = cardinalSprites[index];

        bool onNorthSouth = index == 0 || index == cardinalSprites.Length - 1;
        bearingMask.sprite = onLeft ? leftMaskSprite : rightMaskSprite;
        columnMask.sprite = onLeft ? rightMaskSprite : leftMaskSprite;
        columnMask.gameObject.SetActive(true);
        
        if (onNorthSouth && compassImage.transform.parent != noMaskParent)
        {
            bearingMask.sprite = fullMaskSprite;
            columnMask.gameObject.SetActive(false);
        }

        int highlightIndex = _highlightedAngle < 180
            ? Mathf.RoundToInt(_highlightedAngle / 180f * 8f)
            : Mathf.RoundToInt((360 - _highlightedAngle) / 180f * 8f);
        if (_isHighlightingAngle && index == highlightIndex && !_updatedImageColors)
        {
            compassImage.color = _highlightedColor;
            backgroundColumn.color = _highlightedColor;
            _updatedImageColors = true;
        }
        else if (index != highlightIndex && _updatedImageColors)
        {
            _updatedImageColors = false;
            compassImage.color = _initialColor;
            backgroundColumn.color = _initialColor;
        }
    }

    public void SetHighlightedAngle(float angle, Color highlightColor)
    {
        ClearHighlightedAngle();
        _isHighlightingAngle = true;
        _updatedImageColors = true;
        _highlightedAngle = angle;
        _highlightedColor = highlightColor;
    }

    public void ClearHighlightedAngle()
    {
        _isHighlightingAngle = false;
        compassImage.color = _initialColor;
        backgroundColumn.color = _initialColor;
    }

    public void OnSubDestroyed() { }
}