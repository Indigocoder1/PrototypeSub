using System.Collections;
using PrototypeSubMod.IonGenerator;
using PrototypeSubMod.Upgrades;
using System.Linq;
using PrototypeSubMod.Facilities.Hull.WyrmActions;
using PrototypeSubMod.PowerSystem;
using PrototypeSubMod.UI.AbilitySelection;
using PrototypeSubMod.UI.ActivatedAbilities;
using UnityEngine;

namespace PrototypeSubMod.IonBarrier;

internal class ProtoIonBarrier : ProtoUpgrade, IOnTakeDamage
{
    [SerializeField] private Renderer[] shieldRenderers;
    [SerializeField] private PowerRelay powerRelay;
    [SerializeField] private GameObject lavaLarvaeRoot;
    [SerializeField] private SelectionMenuManager selectionMenuManager;
    [SerializeField] private ActivatedAbilitiesManager abilitiesManager;
    [SerializeField] private FMOD_CustomLoopingEmitter sfxEmitter;
    [SerializeField] private float chargeUseCount;
    [SerializeField] private float powerPerDamage;
    [SerializeField] private float useDuration;
    [SerializeField] private float cooldownDuration;
    [SerializeField] private float maxShieldIntensity;
    [SerializeField] private float defaultReduction;
    [SerializeField] private DamageReductor[] damageReductors;
    [SerializeField] private VoiceNotification shieldsUpNotification;

    [SerializeField, HideInInspector] private float[] multipliers;
    [SerializeField, HideInInspector] private DamageType[] damageTypes;

    private DamageReductor[] serializedDamageReductors;
    private SubRoot subRoot;
    private float targetShieldIntensity;
    private float currentShieldIntensity;
    private float currentImpactIntensity;
    private float damageReductionMultipier;
    private bool shieldActive;
    private bool onCooldown;
    private bool chargesRefunded;

    private void OnValidate()
    {
        multipliers = new float[damageReductors.Length];
        damageTypes = new DamageType[damageReductors.Length];

        for (int i = 0; i < damageReductors.Length; i++)
        {
            multipliers[i] = damageReductors[i].reductionMultiplier;
            damageTypes[i] = damageReductors[i].type;
        }
    }

    private void Start()
    {
        serializedDamageReductors = new DamageReductor[multipliers.Length];
        for (int i = 0; i < multipliers.Length; i++)
        {
            serializedDamageReductors[i] = new(multipliers[i], damageTypes[i]);
        }

        subRoot = GetComponentInParent<SubRoot>();
    }

    private void Update()
    {
        if (subRoot.LOD.IsMinimal()) return;

        HandleShieldProperties();
    }

    private void HandleShieldProperties()
    {
        currentShieldIntensity = Mathf.MoveTowards(currentShieldIntensity, targetShieldIntensity, Time.deltaTime / 2f);
        currentImpactIntensity = Mathf.MoveTowards(currentImpactIntensity, 0, Time.deltaTime / 4f);
        foreach (var rend in shieldRenderers)
        {
            rend.material.SetFloat(ShaderPropertyID._Intensity, currentShieldIntensity);
            rend.material.SetFloat(ShaderPropertyID._ImpactIntensity, currentImpactIntensity);

            if (Mathf.Approximately(currentShieldIntensity, 0) && targetShieldIntensity == 0)
            {
                rend.gameObject.SetActive(false);
            }
        }
    }

    public float GetReductionForType(DamageType type)
    {
        DamageReductor reductor = serializedDamageReductors.FirstOrDefault(r => r.type == type);

        float multiplier = damageReductionMultipier < 0 ? 1 : damageReductionMultipier;

        if (reductor == null)
        {
            return defaultReduction * multiplier;
        }

        return reductor.reductionMultiplier * multiplier;
    }
  
    public void OnTakeDamage(DamageInfo damageInfo)
    {
        if (!upgradeEnabled || !upgradeInstalled) return;

        if (!chargesRefunded)
        {
            chargesRefunded = true;
            powerRelay.AddEnergy(chargeUseCount * PrototypePowerSystem.CHARGE_POWER_AMOUNT, out _);
        }

        if (damageInfo.dealer && damageInfo.dealer.TryGetComponent(out WyrmShootTarget wyrmShootTarget))
        {
            wyrmShootTarget.OnShotParried();
        }
        
        float powerCost = damageInfo.originalDamage * powerPerDamage;
        powerRelay.ConsumeEnergy(powerCost, out _);

        foreach (var rend in shieldRenderers)
        {
            if (!rend.gameObject.activeSelf) continue;

            rend.material.SetVector(ShaderPropertyID._ImpactPosition, damageInfo.position);
            currentImpactIntensity = 1;

            if (damageInfo.dealer != null && damageInfo.dealer.GetComponent<LiveMixin>())
            {
                UWE.CoroutineHost.StartCoroutine(DealDamageOverTime(damageInfo.dealer.GetComponent<LiveMixin>(), 20, 5,
                    DamageType.Electrical));

            }
        }
    }

    private IEnumerator DealDamageOverTime(LiveMixin target, float damage, float duration, DamageType type)
    {
        float startTime = Time.time;
        float dmgPerUpdate = damage / duration;
        while (Time.time < startTime + duration)
        {
            target.TakeDamage(dmgPerUpdate * Time.deltaTime, type: type);
            yield return null;
        }
    }

    private void SetShieldEnabled(bool enabled)
    {
        if (shieldActive == enabled) return;

        SetUpgradeEnabled(enabled);
        
        if (enabled)
        {
            bool couldDraw = powerRelay.ConsumeEnergy(chargeUseCount * PrototypePowerSystem.CHARGE_POWER_AMOUNT, out _);
            if (!couldDraw)
            {
                SetUpgradeEnabled(false);
                return;
            }
            
            ActivateShield();
            subRoot.voiceNotificationManager.PlayVoiceNotification(shieldsUpNotification);
            StartCoroutine(DisableShieldDelayed());
            sfxEmitter.Play();
            chargesRefunded = false;
        }
        else
        {
            var icon = selectionMenuManager.GetAbilityIcons().FirstOrDefault(i => i == (IAbilityIcon)this);
            abilitiesManager.OnAbilityActivatedChanged(icon);
            DeactivateShield();
            StartCoroutine(ResetCooldownDelayed());
            sfxEmitter.Stop();
        }
        
        shieldActive = enabled;
    }

    private void ActivateShield()
    {
        foreach (var lavaLarva in lavaLarvaeRoot.GetComponentsInChildren<LavaLarva>())
        {
            lavaLarva.GetComponent<LiveMixin>().TakeDamage(1, type: DamageType.Electrical);
        }

        foreach (var rend in shieldRenderers)
        {
            rend.gameObject.SetActive(true);
        }

        targetShieldIntensity = maxShieldIntensity;
    }

    private IEnumerator DisableShieldDelayed()
    {
        yield return new WaitForSeconds(useDuration);
        SetShieldEnabled(false);
    }
    
    private IEnumerator ResetCooldownDelayed()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        onCooldown = false;
    }

    private void DeactivateShield()
    {
        targetShieldIntensity = 0;
    }

    public void SetDamageReductionMultiplier(float multiplier)
    {
        damageReductionMultipier = multiplier;
    }

    public override bool OnActivated()
    {
        if (shieldActive || onCooldown) return false;

        if (powerRelay.GetPower() <= 0) return false;
        
        SetShieldEnabled(true);
        return true;
    }

    public override void OnSelectedChanged(bool changed) { }
    public override bool GetCanActivate() => !onCooldown;
}

[System.Serializable]
internal class DamageReductor
{
    public float reductionMultiplier;
    public DamageType type;

    public DamageReductor(float reductionMultiplier, DamageType type)
    {
        this.reductionMultiplier = reductionMultiplier;
        this.type = type;
    }
}