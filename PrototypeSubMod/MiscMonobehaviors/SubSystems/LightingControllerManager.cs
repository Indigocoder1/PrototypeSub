using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class LightingControllerManager : MonoBehaviour
{
    private bool manualLightControlActive;

    public bool ManualLightControlActive() => manualLightControlActive;

    public void SetManualLightControlActive(bool active)
    {
        manualLightControlActive = active;
    }
}