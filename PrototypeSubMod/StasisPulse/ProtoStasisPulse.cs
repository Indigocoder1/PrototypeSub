using System;
using PrototypeSubMod.Upgrades;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Utility;
using PrototypeSubMod.PowerSystem;
using UnityEngine;

namespace PrototypeSubMod.StasisPulse;

internal class ProtoStasisPulse : ProtoUpgrade
{
    private const int FREEZE_STEPS = 8;
    private const int FREEZE_COUNT = 4;
    
    [SerializeField] private AnimationCurve sphereRadius;
    [SerializeField] private Gradient colorOverLifetime;
    [SerializeField] private PowerRelay powerRelay;
    [SerializeField] private FMOD_CustomEmitter activationSfx;
    [SerializeField] private VoiceNotification invalidOperationVoiceline;
    [SerializeField] private VoiceNotification activationVoiceline;
    [SerializeField] private float activationDelay;
    [SerializeField] private int chargeConsumptionAmount;
    [SerializeField] private float cooldownTime;
    [SerializeField] private float sphereGrowTime;
    [SerializeField] private float minFreezeTime;
    [SerializeField] private float maxFreezeTime;
    [SerializeField] private Renderer sphereVisual;

    private float CurrentDiameter => sphereRadius.Evaluate(currentSphereGrowTimeTime / sphereGrowTime);

    private List<FlashingLightHelpers.ShaderVector4ScalerToken> textureSpeedTokens;
    private GameObject freezeFX;
    private GameObject unfreezeFX;
    private SubRoot subRoot;
    
    private float currentCooldownTime;
    private float currentSphereGrowTimeTime;
    private bool deployingLastFrame;
    private bool activating;
    private Material[] materials;
    
    private void OnEnable()
    {
        UWE.CoroutineHost.StartCoroutine(Initialize());
    }

    private void Awake()
    {
        sphereVisual.gameObject.SetActive(false);
    }

    private IEnumerator Initialize()
    {
        subRoot = GetComponentInParent<SubRoot>();
        
        if (freezeFX) yield break;
        
        var rifleTask = CraftData.GetPrefabForTechTypeAsync(TechType.StasisRifle);
        yield return rifleTask;

        var stasisRifle = rifleTask.GetResult();
        var stasisSphere = stasisRifle.GetComponent<StasisRifle>().effectSpherePrefab.GetComponent<StasisSphere>();

        freezeFX = stasisSphere.vfxFreeze;
        unfreezeFX = stasisSphere.vfxUnfreeze;
        
        var stasisMaterials = stasisSphere.GetComponent<Renderer>().materials;
        materials = new Material[stasisMaterials.Length];

        for (int i = 0; i < stasisMaterials.Length; i++)
        {
            materials[i] = MaterialUtils.StasisFieldMaterial;
        }

        sphereVisual.materials = materials;
        sphereVisual.GetComponent<MeshFilter>().mesh = stasisSphere.GetComponent<MeshFilter>().mesh;
        textureSpeedTokens = FlashingLightHelpers.CreateUberShaderVector4ScalerTokens(sphereVisual.materials[0], sphereVisual.materials[1]);

        MiscSettings.isFlashesEnabled.changedEvent.AddHandler(this, OnFlashesEnabledChanged);
        UpdateTextureSpeed();

        sphereVisual.enabled = true;
        currentSphereGrowTimeTime = sphereGrowTime;
        sphereVisual.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        sphereVisual.gameObject.SetActive(currentSphereGrowTimeTime < sphereGrowTime);
        UpdateMaterials();

        if (!upgradeInstalled)
        {
            return;
        }

        if (currentCooldownTime > 0)
        {
            currentCooldownTime -= Time.deltaTime;
            return;
        }

        HandleSphereSize();
    }

    private void UpdateMaterials()
    {
        if (sphereVisual.materials.Length != 2) return;

        Color color = colorOverLifetime.Evaluate(currentSphereGrowTimeTime / sphereGrowTime);
        sphereVisual.materials[0].SetColor(ShaderPropertyID._Color, color);
        sphereVisual.materials[1].SetColor(ShaderPropertyID._Color, color);
    }

    private void HandleSphereSize()
    {
        if (currentSphereGrowTimeTime < sphereGrowTime)
        {
            currentSphereGrowTimeTime += Time.deltaTime;
            sphereVisual.transform.localScale = Vector3.one * CurrentDiameter;
            deployingLastFrame = true;
        }
        else if (deployingLastFrame)
        {
            currentCooldownTime = cooldownTime;
            deployingLastFrame = false;
        }
    }

    public void OnHitObject(Collider collider)
    {
        if (Plugin.GlobalSaveData.prototypeDestroyed) return;
        
        TryFreeze(collider);
    }
    
    private void TryFreeze(Collider collider)
    {
        if (Player.mainCollider == collider) return;
        
        Rigidbody rigidbody = collider.GetComponentInParent<Rigidbody>();
        if (!rigidbody) return;

        var hitSubRoot = rigidbody.GetComponentInParent<SubRoot>();
        if (hitSubRoot && hitSubRoot == subRoot) return;
        
        if (rigidbody.isKinematic) return;

        if (rigidbody.TryGetComponent<ProtoStasisFreeze>(out _)) return;

        if (collider == Player.mainCollider) return;
        
        var freeze = rigidbody.gameObject.AddComponent<ProtoStasisFreeze>();
        freeze.SetFreezeTimes(minFreezeTime, maxFreezeTime);
        freeze.SetUnfreezeVF(unfreezeFX);

        Utils.PlayOneShotPS(freezeFX, rigidbody.transform.position, Quaternion.identity);
    }

    private void OnFlashesEnabledChanged(Utils.MonitoredValue<bool> isFlashesEnabled)
    {
        UpdateTextureSpeed();
    }

    private void UpdateTextureSpeed()
    {
        if (MiscSettings.flashes)
        {
            textureSpeedTokens.RestoreScale();
            return;
        }

        textureSpeedTokens.SetScale(0.1f);
    }

    public void ActivateSphere()
    {
        if (!upgradeInstalled) return;

        subRoot.voiceNotificationManager.PlayVoiceNotification(activationVoiceline);

        activationSfx.Play();
        MainCameraControl.main.ShakeCamera(1, activationDelay + 0.1f, MainCameraControl.ShakeMode.BuildUp, 2);
        Invoke(nameof(StartGrow), activationDelay);
        activating = true;
    }

    private void StartGrow()
    {
        currentSphereGrowTimeTime = 0;
        deployingLastFrame = false;
        activating = false;

        powerRelay.ConsumeEnergy(PrototypePowerSystem.CHARGE_POWER_AMOUNT * chargeConsumptionAmount, out _);
    }

    public override bool GetUpgradeEnabled() => upgradeInstalled;

    public override bool OnActivated()
    {
        if (currentSphereGrowTimeTime < sphereGrowTime || currentCooldownTime > 0)
        {
            subRoot.voiceNotificationManager.PlayVoiceNotification(invalidOperationVoiceline);
            return false;
        }

        if (activating) return false;
        
        if (powerRelay.GetPower() < PrototypePowerSystem.CHARGE_POWER_AMOUNT * chargeConsumptionAmount)
        {
            return false;
        }
        
        ActivateSphere();
        return true;
    }

    public override void OnSelectedChanged(bool changed) { }
}
