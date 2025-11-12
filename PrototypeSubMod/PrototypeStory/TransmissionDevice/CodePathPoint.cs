using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionDevice;

public class CodePathPoint : MonoBehaviour
{
    [SerializeField] private TransmissionDeviceUINumber numberButton;

    public TransmissionDeviceUINumber GetNumberButton() => numberButton;
}