using System;
using System.Collections;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.PowerSystem;
using SubLibrary.UI;
using UnityEngine;

namespace PrototypeSubMod.UI.PowerDisplay;

public class ProtoChargeDisplay : MonoBehaviour, IUIElement
{
    [SerializeField] private FMODAsset destroyChargeSFX;
    [SerializeField] private VoiceNotification lowPowerNotification;
    [SerializeField] private VoiceNotification criticalPowerNotification;
    [SerializeField] private PrototypePowerSystem powerSystem;
    [SerializeField] private OnModifyPowerEvent onModifyPower;
    [SerializeField] private ProtoChargeIcon[] icons;
    [SerializeField] private Color normalCol;
    [SerializeField] private Color lowPowerCol;

    private VoiceNotificationManager voiceNotificationManager;
    private int chargesLastCheck;
    
    private void Start()
    {
        UWE.CoroutineHost.StartCoroutine(Initialize());
        voiceNotificationManager = GetComponentInParent<VoiceNotificationManager>();
    }

    private IEnumerator Initialize()
    {
        yield return new WaitForEndOfFrame();
        
        onModifyPower.onModifyPower += UpdateCharges;
        powerSystem.onReorderSources += RegenerateCharges;
        powerSystem.equipment.onAddItem += _ => RegenerateCharges();

        RegenerateCharges();
    }
    
    private void OnEnable()
    {
        RegenerateCharges();
    }

    public void UpdateUI() { }

    public void OnSubDestroyed()
    {
        UpdateCharges(0);
    }

    private void RegenerateCharges()
    {
        foreach (var icon in icons)
        {
            icon.gameObject.SetActive(false);
        }
        
        if (powerSystem.equipment?.GetItemInSlot(PrototypePowerSystem.SLOT_NAMES[0]) == null) return;

        var currentSource = powerSystem.GetPowerSources()[0];
        for (int i = 0; i < currentSource.GetRemainingCharges(); i++)
        {
            icons[i].gameObject.SetActive(true);
            icons[i].SetColor(normalCol);
        }
        
        chargesLastCheck = currentSource.GetRemainingCharges();
    }
    
    public void UpdateCharges(float chargeChange)
    {
        var currentSource = powerSystem.GetPowerSources()[0];
        if (currentSource == null || powerSystem.GetInstalledSourceCount() == 0) return;
        
        int remainingCharges = currentSource.GetRemainingCharges();

        var chargeIcon = icons[0];
        if (currentSource.GetCurrentChargePower01() <= 0.25f)
        {
            chargeIcon.SetColor(lowPowerCol);
        }
        else if (chargeChange > 0)
        {
            chargeIcon.SetColor(normalCol);
        }

        if (chargesLastCheck != remainingCharges)
        {
            if (remainingCharges < chargesLastCheck)
            {
                FMODUWE.PlayOneShot(destroyChargeSFX, Player.main.transform.position, 0.5f);
            }

            HandleLowPowerLines(remainingCharges);
            RegenerateCharges();
        }
        
        chargesLastCheck = remainingCharges;
    }

    private void HandleLowPowerLines(int remainingCharges)
    {
        if (remainingCharges < chargesLastCheck && powerSystem.GetInstalledSourceCount() == 1)
        {
            if (remainingCharges == 3)
            {
                voiceNotificationManager.PlayVoiceNotification(lowPowerNotification);
            }
            else if (remainingCharges == 1)
            {
                voiceNotificationManager.PlayVoiceNotification(criticalPowerNotification);
            }
        }
    }
}