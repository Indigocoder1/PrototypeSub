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