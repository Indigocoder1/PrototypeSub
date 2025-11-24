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
    [SerializeField] private Image bearingMask;
    [SerializeField] private Image columnMask;
    [SerializeField] private Sprite rightMaskSprite;
    [SerializeField] private Sprite leftMaskSprite;
    [SerializeField] private Sprite fullMaskSprite;
    [SerializeField] private Sprite[] cardinalSprites;

    private void Start()
    {
        bool onLeft = subRoot.transform.eulerAngles.y > 180;
        bearingMask.sprite = onLeft ? leftMaskSprite : rightMaskSprite;
        columnMask.sprite = onLeft ? rightMaskSprite : leftMaskSprite;
        columnMask.gameObject.SetActive(true);
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
    }

    public void OnSubDestroyed() { }
}