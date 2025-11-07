using PrototypeSubMod.IonGenerator;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using PrototypeSubMod.Upgrades;
using System.Collections.Generic;
using PrototypeSubMod.PowerSystem;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.LightDistortionField;

internal class CloakEffectHandler : ProtoUpgrade
{
    [SaveStateReference]
    public static List<CloakEffectHandler> EffectHandlers = new();

    [Header("Shader Parameters")]
    public Shader shader;
    public Transform ovoid;

    [Header("Colors")]
    public Color color;
    public Color distortionColor;
    public Color interiorColor;
    public Color vignetteColor;
    public float falloffMin;
    public float falloffMax;

    [Header("Distortion")]
    public float falloffMultiplier;
    public float distortionBoundaryMin;
    public float distortionBoundaryMax;
    public float distortionBoundaryOffset;
    public float distortionAmplitude;

    [Header("Vignette")]
    public float vignetteIntensity;
    public float vignetteSmoothness;
    public float vignetteOffset;
    public float vignetteFadeInDist;

    [Header("Oscillation")]
    public float oscillationFrequency;
    public float oscillationAmplitude;
    public float oscillationSpeed;
    public int waveCount;
    public float frequencyIncrease;
    public float amplitudeFalloff;

    [Header("Animation")]
    public float scaleTime;
    public AnimationCurve scaleOverTime;

    [Header("Sound Values")]
    public float soundMultiplier;

    [Header("Power Draw")]
    [SerializeField] private PowerRelay powerRelay;
    [SerializeField] private float secondsToConsumeCharge;
    
    [Header("Sound")]
    [SerializeField] private VoiceNotificationManager  voiceNotificationManager;
    [SerializeField] private VoiceNotification activateCloakNotif;
    [SerializeField] private VoiceNotification invalidOpNotification;
    [SerializeField] private FMOD_CustomLoopingEmitter distortionActiveSFX;
    [SerializeField] private FMOD_CustomEmitter distortionActivateSFX;

    [Header("Miscellaneous")]
    [SerializeField] private FMOD_CustomLoopingEmitter emitter;
    public ProtoIonGenerator ionGenerator;
    [SerializeField] private EmissionColorController emissionController;
    [SerializeField] private Color emissiveColor = Color.black;

    private float TargetScaleMultiplier => GetIsCloaking() ? 1 : 0;

    private Vector3 originalScale;
    private float currentScaleTime;
    private bool isDirty = true;

    private void Awake()
    {
        shader = Plugin.ShadersAssetBundle.LoadAsset<Shader>(shader.name.Split('/')[^1]);
    }
    
    private void Start()
    {
        originalScale = ovoid.localScale;
        ovoid.localScale = originalScale * TargetScaleMultiplier;

        var distortionApplier = Camera.main.GetComponent<LightDistortionApplier>();
        distortionApplier.RegisterCloakHandler(this);
    }

    private void OnValidate()
    {
        isDirty = true;
    }

    private void Update()
    {
        // Map [0, 1] to [-1, 1]
        float delta = Time.deltaTime * ((TargetScaleMultiplier * 2) - 1);

        bool growCheck = TargetScaleMultiplier == 1 && currentScaleTime < scaleTime;
        bool shrinkCheck = TargetScaleMultiplier == 0 && currentScaleTime > 0;
        if (growCheck || shrinkCheck)
        {
            currentScaleTime += delta;
        }

        ovoid.localScale = originalScale * scaleOverTime.Evaluate(currentScaleTime / scaleTime);

        if (GetIsCloaking())
        {
            bool couldConsume = powerRelay.ConsumeEnergy(PrototypePowerSystem.CHARGE_POWER_AMOUNT / secondsToConsumeCharge * Time.deltaTime,
                out _);

            if (!couldConsume)
            {
                SetUpgradeEnabled(false);
            }
        }
    }

    public bool IsInsideOvoid(Vector3 point)
    {
        if (!upgradeEnabled) return false;

        Vector3 localPoint = point - ovoid.position;

        localPoint = Quaternion.Inverse(ovoid.rotation) * localPoint;

        Vector3 normalizedPoint = Divide(localPoint, ovoid.localScale);

        return normalizedPoint.sqrMagnitude < 1;
    }

    public Vector3 GetClosestPointOnSurface(Vector3 point, float scalarOffset = 0f)
    {
        Vector3 direction = point - ovoid.position;
        var localDir = Quaternion.Inverse(ovoid.rotation) * direction;
        
        float magnitude = Divide(localDir, originalScale).magnitude;
        var pointOnSurf = ovoid.position + direction * ((1 / magnitude));

        return pointOnSurf + direction.normalized * scalarOffset;
    }

    public Vector3 GetContinuousPointOnSurface(float scalarOffset = 0)
    {
        float sin = Mathf.Sin(Time.time * 0.01f);
        var randVector = new Vector3(Mathf.Cos(Time.time * 0.01f), sin, sin);
        return GetClosestPointOnSurface(ovoid.position + randVector, scalarOffset);
    }

    private Vector3 Divide(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
    }

    public bool GetIsCloaking()
    {
        return !ionGenerator.GetUpgradeEnabled() && upgradeEnabled && upgradeInstalled;
    }

    public float GetTargetScale()
    {
        return TargetScaleMultiplier;
    }

    public override void SetUpgradeEnabled(bool enabled)
    {
        if (enabled && !upgradeEnabled)
        {
            emissionController.RegisterTempColor(new EmissionColorController.EmissionRegistrarData(this, emissiveColor, 20));
            emitter.Play();
            voiceNotificationManager.PlayVoiceNotification(activateCloakNotif, false);
            distortionActiveSFX.Play();
            distortionActivateSFX.Play();
        }
        else if (!enabled && upgradeEnabled)
        {
            emissionController.RemoveTempColor(this);
            emitter.Stop();
            distortionActiveSFX.Stop();
        }
        
        base.SetUpgradeEnabled(enabled);
    }

    private void OnEnable()
    {
        EffectHandlers.Add(this);
    }

    private void OnDisable()
    {
        EffectHandlers.Remove(this);
    }

    public bool GetIsDirty() => isDirty;
    public void ClearDirty() => isDirty = false;

    public override bool OnActivated()
    {
        if (ionGenerator.GetUpgradeEnabled())
        {
            voiceNotificationManager.PlayVoiceNotification(invalidOpNotification);
            return false;
        }
        
        SetUpgradeEnabled(!upgradeEnabled);
        return true;
    }

    public override void OnSelectedChanged(bool changed) { }
    public override bool GetCanActivate() => !ionGenerator.GetUpgradeEnabled();
}