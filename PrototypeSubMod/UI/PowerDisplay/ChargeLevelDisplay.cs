using PrototypeSubMod.PowerSystem;
using SubLibrary.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.UI.PowerDisplay;

public class ChargeLevelDisplay : MonoBehaviour, IUIElement
{
    [SerializeField] private PrototypePowerSystem powerSystem;
    [SerializeField] private Image chargeBar;

    private bool destroyed;

    public void UpdateUI()
    {
        var source0 = powerSystem.GetPowerSources()[0];
        if (!source0.HasBattery() || destroyed)
        {
            chargeBar.fillAmount = 0;
            return;
        }

        float charge01 = source0.GetCurrentChargePower01();
        chargeBar.fillAmount = charge01;
    }

    public void OnSubDestroyed()
    {
        destroyed = true;
    }
}