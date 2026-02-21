using System;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.Upgrades;
using UnityEngine;

namespace PrototypeSubMod.ProtoStrafe;

internal class ProtoStrafe : ProtoUpgrade
{
    [SerializeField] private CyclopsMotorMode motorMode;
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private SubControl subControl;
    [SerializeField] private FMOD_CustomEmitter strafeOnSfx;
    [SerializeField] private FMOD_CustomEmitter strafeOffSfx;

    private PilotingChair chair;

    private void Start()
    {
        chair = subControl.GetComponentInChildren<PilotingChair>();
    }
    
    private void Update()
    {
        if (Player.main.currChair != chair) return;
        
        if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
        {
            SetUpgradeEnabled(!upgradeEnabled);
            if (upgradeEnabled)
            {
                strafeOnSfx.Play();
            }
            else
            {
                strafeOffSfx.Play();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!upgradeEnabled) return;
        
        rigidbody.AddForce(transform.right * (motorMode.motorModeSpeeds[1] * subControl.throttle.x), ForceMode.Acceleration);
    }
    
    public override bool OnActivated()
    {
        SetUpgradeEnabled(!upgradeEnabled);
        return true;
    }

    public override void OnSelectedChanged(bool changed) { }
}