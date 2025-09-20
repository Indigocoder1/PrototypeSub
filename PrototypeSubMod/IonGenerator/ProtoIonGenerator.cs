using PrototypeSubMod.MotorHandler;
using PrototypeSubMod.Upgrades;
using System.Collections;
using PrototypeSubMod.PowerSystem;
using UnityEngine;

namespace PrototypeSubMod.IonGenerator;

internal class ProtoIonGenerator : ProtoUpgrade
{
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private ProtoMotorHandler motorHandler;
    [SerializeField] private VoiceNotification overheatNotification;
    [SerializeField] private float secondsToFillCharge;
    [SerializeField] private float activeNoiseValue;
    [SerializeField] private FMOD_CustomEmitter generatorStart;
    [SerializeField] private FMOD_CustomEmitter generatorStop;
    [SerializeField] private FMOD_CustomEmitter generatorLoop;
    
    private float energyMultiplier = 1;
    private float chargePerSec;

    private void Start()
    {
        chargePerSec = PrototypePowerSystem.CHARGE_POWER_AMOUNT / secondsToFillCharge;
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
        }
        else
        {
            motorHandler.RemoveOverrideNoiseValue(this);
        }
    }

    public void SetEnergyMultiplier(float multiplier)
    {
        energyMultiplier = multiplier;
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

    public override void OnSelectedChanged(bool changed) { }
}