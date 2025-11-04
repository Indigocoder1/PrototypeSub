using System;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationDirectionIndicator : MonoBehaviour
{
    [SerializeField] private Image headingSprite;
    [SerializeField] private Image backgroundSprite;
    [SerializeField] private Transform leftHalfMask;
    [SerializeField] private Transform rightHalfMask;
    [SerializeField] private CalibrationRunManager runManager;
    [SerializeField] private CompassDirection[] compassDirections;

    [HideInInspector, SerializeField]
    public Sprite[] headingSprites;
    [HideInInspector, SerializeField]
    public bool[] leftHalves;

    private CompassDirection[] serializedDirections;
    
    private void OnValidate()
    {
        headingSprites = new Sprite[compassDirections.Length];
        leftHalves = new bool[compassDirections.Length];
        for (int i = 0; i < compassDirections.Length; i++)
        {
            headingSprites[i] = compassDirections[i].heading;
            leftHalves[i] = compassDirections[i].isLeftHalf;
        }
    }

    private void Start()
    {
        serializedDirections = new CompassDirection[headingSprites.Length];
        for (int i = 0; i < headingSprites.Length; i++)
        {
            serializedDirections[i] = new CompassDirection(headingSprites[i], leftHalves[i]);
        }

        runManager.onPointReached += OnPointReached;
    }

    private void OnPointReached(int index)
    {
        if (index < 0 || index > serializedDirections.Length - 1) return;

        headingSprite.sprite = serializedDirections[index].heading;
        bool onLeft = serializedDirections[index].isLeftHalf;

        headingSprite.transform.SetParent(onLeft ? leftHalfMask : rightHalfMask);
        backgroundSprite.transform.SetParent(onLeft ? rightHalfMask : leftHalfMask);
    }
}

[Serializable]
public struct CompassDirection
{
    public Sprite heading;
    public bool isLeftHalf;

    public CompassDirection(Sprite heading, bool isLeftHalf)
    {
        this.heading = heading;
        this.isLeftHalf = isLeftHalf;
    }
}