using PrototypeSubMod.MotorHandler;
using PrototypeSubMod.Upgrades;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using PrototypeSubMod.PowerSystem;
using PrototypeSubMod.UI.PowerDisplay;
using UnityEngine;

namespace PrototypeSubMod.IonGenerator;

internal class ProtoIonGenerator : ProtoUpgrade
{
    [SerializeField] private ProtoMotorHandler motorHandler;
    [SerializeField] private float secondsToFillCharge;
    [SerializeField] private float activeNoiseValue;
    [SerializeField] private FMOD_CustomEmitter generatorStart;
    [SerializeField] private FMOD_CustomEmitter generatorStop;
    [SerializeField] private FMOD_CustomEmitter generatorLoop;
    [SerializeField] private EmissionColorController emissionController;
    [SerializeField] private PrototypePowerSystem powerSystem;

    private ProtoChargeDisplay chargeDisplay;
    private float chargePerSec;

    private void Start()
    {
        chargePerSec = PrototypePowerSystem.CHARGE_POWER_AMOUNT / secondsToFillCharge;
        chargeDisplay = GetComponentInParent<SubRoot>().GetComponentInChildren<ProtoChargeDisplay>(true);
    }

    private void Update()
    {
        if (!upgradeInstalled)
        {
            motorHandler.SetAllowedToMove(true);
            return;
        }

        motorHandler.SetAllowedToMove(!upgradeEnabled);
        if (upgradeEnabled)
        {
            motorHandler.AddOverrideNoiseValue(new ProtoMotorHandler.ValueRegistrar(this, activeNoiseValue));
            var firstItem = powerSystem.GetPowerSources()[0];
            if (firstItem != null)
            {
                float chargeDelta = chargePerSec * Time.deltaTime;
                firstItem.ModifyPower(chargeDelta, out _);
                chargeDisplay.UpdateCharges(chargeDelta);
            }
        }
        else
        {
            motorHandler.RemoveOverrideNoiseValue(this);
        }
    }

    public override bool OnActivated()
    {
        if (!upgradeInstalled) return false;
        
        SetUpgradeEnabled(!upgradeEnabled);

        if (upgradeEnabled)
        {
            generatorLoop.Play();
            generatorStart.Play();
        }
        else
        {
            generatorLoop.Stop();
            generatorStop.Play();
        }

        return true;
    }

    public override void SetUpgradeEnabled(bool enabled)
    {
        base.SetUpgradeEnabled(enabled);
        if (enabled)
        {
            emissionController.RegisterTempColor(new EmissionColorController.EmissionRegistrarData(this, Color.black));
        }
        else
        {
            emissionController.RemoveTempColor(this);
        }
    }

    public override void OnSelectedChanged(bool changed) { }
}