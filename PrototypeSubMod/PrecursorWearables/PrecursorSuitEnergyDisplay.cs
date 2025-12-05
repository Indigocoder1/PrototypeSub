using PrototypeSubMod.Factors;
using UnityEngine;

namespace PrototypeSubMod.PrecursorWearables;

public class PrecursorSuitEnergyDisplay : MonoBehaviour, IBattery
{
    private FactorIonManager ionManager;

    private void Awake()
    {
        ionManager = GetComponent<FactorIonManager>();
    }

    public string GetChargeValueText()
    {
        var charge01 = ionManager.GetNormalizedCharge();
        return Language.main.GetFormat("BatteryCharge", charge01, Mathf.RoundToInt(ionManager.GetCurrentEnergy()), ionManager.GetMaxEnergy());
    }

    public float charge {
        get => ionManager.GetCurrentEnergy();
        set { }
    }

    public float capacity => ionManager.GetMaxEnergy();
}