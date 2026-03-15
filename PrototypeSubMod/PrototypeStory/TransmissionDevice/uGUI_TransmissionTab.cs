using Nautilus.Utility;
using System;
using PrototypeSubMod.Registration;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class uGUI_TransmissionTab : uGUI_PDATab
{
    public event Action onTransmissionComplete;
    public event Action onTabOpened;
    public event Action onTabClosed;
    
    [SerializeField] private TransmissionDeviceUINumber[] deviceNumbers;

    private CanvasGroup canvasGroup;
    private bool onCorrectSequence;
    
    private void Start()
    {
        foreach (var deviceNumber in deviceNumbers)
        {
            deviceNumber.onNumberChanged += OnNumberChanged;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnNumberChanged()
    {
        onCorrectSequence = false;
        
        foreach (var deviceNumber in deviceNumbers)
        {
            if (!deviceNumber.OnCorrectNumber()) return;            
        }

        onCorrectSequence = true;
    }

    private bool InTransmissionSite()
    {
        return Player.main.GetBiomeString() == BiomeRegisterer.TransmissionSiteBiome;
    }

    public void OnTransmitClicked()
    {
        if (!onCorrectSequence || !InTransmissionSite())
        {
            FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("NoPower"), Player.main.transform.position);
            return;
        }
        
        onTransmissionComplete?.Invoke();
    }

    public TransmissionDeviceUINumber[] GetNumbers() => deviceNumbers;

    public override void OnOpenPDA(PDATab tab, bool explicitly)
    {
        if (tab != Plugin.TransmissionEntryTab) return;
        
        base.OnOpenPDA(tab, explicitly);
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        onTabOpened?.Invoke();
    }

    public override void OnClosePDA()
    {
        base.OnClosePDA();
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        onTabClosed?.Invoke();
    }
}