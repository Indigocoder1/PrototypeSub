using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class ManuallyControllerVolume : MonoBehaviour
{
    [SerializeField] private AtmosphereVolume volume;

    private IEnumerator Start()
    {
        yield return null;
        volume.CancelInvoke(nameof(volume.CheckTriggerExit));
    }

    public void PushSettings()
    {
        volume.PushSettings();
    }

    public void PopSettings()
    {
        volume.PopSettings();
    }
}