using System;
using PrototypeSubMod.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Credits;

public class ProtoScreenFadeManager : MonoBehaviour
{
    [SaveStateReference]
    public static ProtoScreenFadeManager instance;
    
    [SerializeField] private Image fadeImage;

    private bool fading;
    private float currentFadeTime;
    private float targetFadeTime;
    private Color targetColor = Color.clear;
    private Color previousColor;

    private void Awake()
    {
        if (instance != null) throw new Exception("More than one ProtoScreenFadeManager in scene.");
        
        instance = this;
    }
    
    private void Start()
    {
        fadeImage.color = Color.clear;
    }

    private void Update()
    {
        if (!fading) return;

        if (currentFadeTime < targetFadeTime)
        {
            currentFadeTime += Time.deltaTime;
        }
        else
        {
            fading = false;
        }
        
        fadeImage.color = Color.Lerp(previousColor, targetColor, currentFadeTime / targetFadeTime);
    }

    public void FadeIn(float duration)
    {
        fading = true;
        currentFadeTime = 0;
        targetFadeTime = duration;
        previousColor = targetColor;
        targetColor = Color.black;
    }
    
    public void FadeOut(float duration)
    {
        fading = true;
        currentFadeTime = 0;
        targetFadeTime = duration;
        previousColor = targetColor;
        targetColor = Color.clear;
    }
}