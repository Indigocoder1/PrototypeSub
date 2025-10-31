using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class uGUI_TransmissionTab : uGUI_PDATab
{
    [SerializeField] private TransmissionDeviceUINumber[] deviceNumbers;
    
    private void Start()
    {
        foreach (var deviceNumber in deviceNumbers)
        {
            deviceNumber.onNumberChanged += OnNumberChanged;
        }
    }

    private void OnNumberChanged()
    {
        foreach (var deviceNumber in deviceNumbers)
        {
            if (!deviceNumber.OnCorrectNumber()) return;            
        }

        ErrorMessage.AddError("Correct sequence entered!");
    }
}