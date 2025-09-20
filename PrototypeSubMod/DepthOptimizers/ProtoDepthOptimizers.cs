using PrototypeSubMod.PowerSystem;
using PrototypeSubMod.PressureConverters;
using PrototypeSubMod.Upgrades;
using UnityEngine;

namespace PrototypeSubMod.DepthOptimizers;

internal class ProtoDepthOptimizers : ProtoUpgrade, IPowerModifier
{
    private static readonly int HydrolockEnabled = Animator.StringToHash("HydrolockEnabled");
    
    [SerializeField] private CrushDamage crushDamage;
    [SerializeField] private float activationDepth;
    [SerializeField] private int chargeReduction;
    [SerializeField] private float passiveChargeMultiplier;
    [SerializeField] private FMOD_CustomEmitter depthOptimizersActivate;
    [SerializeField] private FMOD_CustomEmitter depthOptimizersDeactivate;
    [SerializeField] private Animator hydrolockController;

    private float originalCrushDepth;
    
    private void Start()
    {
        originalCrushDepth = crushDamage.crushDepth;
    }

    public override void SetUpgradeEnabled(bool enabled)
    {
        if (enabled && !upgradeEnabled)
        {
            depthOptimizersActivate.Play();
        }
        else if (!enabled && upgradeEnabled)
        {
            depthOptimizersDeactivate.Play();
        }
        
        base.SetUpgradeEnabled(enabled);

        if (enabled)
        {
            crushDamage.crushDepth = 9000;
        }
        else
        {
            crushDamage.crushDepth = originalCrushDepth;
        }
        
        hydrolockController.SetBool(HydrolockEnabled, enabled);
    }

    public override bool GetUpgradeEnabled() => upgradeEnabled;

    public override bool OnActivated()
    {
        SetUpgradeEnabled(!upgradeEnabled);
        return true;
    }

    public override void OnSelectedChanged(bool changed) { }

    public void ModifyPowerDrawn(ref float amount)
    {
        if (crushDamage.GetDepth() < activationDepth) return;

        if (amount >= 0) return;

        if (-amount > PrototypePowerSystem.CHARGE_POWER_AMOUNT)
        {
            amount += PrototypePowerSystem.CHARGE_POWER_AMOUNT * chargeReduction;
            return;
        }

        amount *= passiveChargeMultiplier;
    }
}
