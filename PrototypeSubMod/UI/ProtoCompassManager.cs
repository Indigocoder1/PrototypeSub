using System;
using SubLibrary.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.UI;

public class ProtoCompassManager : MonoBehaviour, IUIElement
{
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private Image compassImage;
    [SerializeField] private Image backgroundBar;
    [SerializeField] private Transform noMaskParent;
    [SerializeField] private Transform leftHalfMask;
    [SerializeField] private Transform rightHalfMask;
    [SerializeField] private Sprite[] cardinalSprites;

    private void Start()
    {
        bool onLeft = subRoot.transform.eulerAngles.y > 180;
        compassImage.transform.SetParent(onLeft ? leftHalfMask : rightHalfMask);
        backgroundBar.transform.SetParent(onLeft ? rightHalfMask : leftHalfMask);
        backgroundBar.gameObject.SetActive(true);
    }

    public void UpdateUI()
    {
        int index = Mathf.RoundToInt(subRoot.transform.eulerAngles.y / 360 * cardinalSprites.Length);
        index %= cardinalSprites.Length;
        bool onLeft = subRoot.transform.eulerAngles.y is > 180 and < 360;

        compassImage.sprite = cardinalSprites[index];

        bool onNorthSouth = index == 0 || index == Mathf.RoundToInt(cardinalSprites.Length / 2f);
        compassImage.transform.SetParent(onLeft ? leftHalfMask : rightHalfMask);
        backgroundBar.transform.SetParent(onLeft ? rightHalfMask : leftHalfMask);
        backgroundBar.gameObject.SetActive(true);
        
        if (onNorthSouth && compassImage.transform.parent != noMaskParent)
        {
            compassImage.transform.SetParent(noMaskParent);
            backgroundBar.gameObject.SetActive(false);
        }
    }

    public void OnSubDestroyed() { }
}