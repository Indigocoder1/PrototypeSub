using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class uGUI_TransmissionTab : uGUI_PDATab
{
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

    public void OnTransmitClicked()
    {
        if (!onCorrectSequence)
        {
            ErrorMessage.AddError("Incorrect sequence!");
            return;
        }
        
        ErrorMessage.AddError("Transmission complete");
    }

    public override void OnOpenPDA(PDATab tab, bool explicitly)
    {
        if (tab != Plugin.TransmissionEntryTab) return;
        
        base.OnOpenPDA(tab, explicitly);
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }

    public override void OnClosePDA()
    {
        base.OnClosePDA();
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }
}