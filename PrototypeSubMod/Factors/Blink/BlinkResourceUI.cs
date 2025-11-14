using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Factors.Blink;

public class BlinkResourceUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image resourceBar;

    private bool uiOpen;

    private void Start()
    {
        canvasGroup.alpha = 0;
    }

    public void OpenUI(Blink owner)
    {
        StopAllCoroutines();
        StartCoroutine(FadeToAlpha(1, owner.resourceFadeInTime));
        uiOpen = true;
    }
    
    public void CloseUI(Blink owner)
    {
        StopAllCoroutines();
        StartCoroutine(FadeToAlpha(0, owner.resourceFadeOutTime, owner.resourceBarFadeDelay));
        uiOpen = false;
    }

    public bool GetUIOpen() => uiOpen;

    private IEnumerator FadeToAlpha(float alpha, float time, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        
        float currentTime = 0;
        float startAlpha = canvasGroup.alpha;
        while (currentTime < time)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, alpha, currentTime / time);
            currentTime += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = alpha;
    }

    public void SetFillAmount(float amount)
    {
        resourceBar.fillAmount = amount;
    }
}