using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class uGUI_TransmissionTab : uGUI_PDATab
{
    [SerializeField] private TransmissionDeviceUINumber[] deviceNumbers;

    private CanvasGroup canvasGroup;
    
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
        foreach (var deviceNumber in deviceNumbers)
        {
            if (!deviceNumber.OnCorrectNumber()) return;            
        }

        ErrorMessage.AddError("Correct sequence entered!");
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