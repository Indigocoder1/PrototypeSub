using Nautilus.Utility;
using System;
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

    public bool InTransmissionSite()
    {
        var playerPos = Player.main.transform.position;
        var sitePos = Plugin.TransmissionSitePos;
        var distance = Vector3.Distance(playerPos, sitePos);
        if (distance > 150f)
        {
            ErrorMessage.AddError($"Incorrect sequence or not in transmission site! Distance: {distance}");
            return false;
        }
        return true;
    }

    public void OnTransmitClicked()
    {
        if (!onCorrectSequence || !InTransmissionSite())
        {
            FMODUWE.PlayOneShot(AudioUtils.GetFmodAsset("NoPower"), Player.main.transform.position);
            return;
        }

        ErrorMessage.AddError("Transmission complete");
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