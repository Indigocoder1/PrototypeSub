using System.Collections;
using PrototypeSubMod.PowerSystem;
using PrototypeSubMod.UI.AbilitySelection;
using PrototypeSubMod.UI.ActivatedAbilities;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

internal class ToggleMinimap : MonoBehaviour, IAbilityIcon
{
    [SerializeField] private Sprite minimapSprite;
    [SerializeField] private FMOD_CustomEmitter nearfieldSFX;
    
    [Header("Power Draw")]
    [SerializeField] private PowerRelay powerRelay;
    [SerializeField] private float secondsToConsumeCharge;

    private ProtoSonarVFXManager sonarVFX;

    private void Start()
    {
        UWE.CoroutineHost.StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        yield return new WaitForEndOfFrame();
        sonarVFX = Camera.main.gameObject.GetComponent<ProtoSonarVFXManager>();
    }
    
    // Called by BroadcastMessage in SubRoot.OnPlayerExited
    public void SaveEngineStateAndPowerDown()
    {
        if (sonarVFX.activated)
        {
            sonarVFX.ToggleActivated();
            nearfieldSFX.Stop();
        }
        GetComponentInParent<SubRoot>().GetComponentInChildren<TetherManager>(true)
            .UpdateIcon(this);
    }

    private void Update()
    {
        if (sonarVFX == null || !sonarVFX.activated) return;
        
        bool couldConsume = powerRelay.ConsumeEnergy(PrototypePowerSystem.CHARGE_POWER_AMOUNT / secondsToConsumeCharge * Time.deltaTime,
            out _);

        if (!couldConsume)
        {
            if (!sonarVFX) return;
            sonarVFX.SetActivated(false);
        }
    }

    public bool OnActivated()
    {
        sonarVFX.ToggleActivated();

        if (sonarVFX.activated)
        {
            nearfieldSFX.Play();
        }
        else
        {
            nearfieldSFX.Stop();
        }
        
        return true;
    }

    public void OnSelectedChanged(bool changed) { }

    public bool GetActive()
    {
        return sonarVFX.activated;
    }

    public bool GetCanActivate() => true;
    public bool GetShouldShow() => true;
    public bool GetIsInstalled() => true;
    public Sprite GetSprite() => minimapSprite;
    public TechType GetTechType() => TechType.None;
}
