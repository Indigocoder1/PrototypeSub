using PrototypeSubMod.Interfaces;
using PrototypeSubMod.SaveData;
using SubLibrary.SaveData;
using System;
using System.Collections;
using System.Collections.Generic;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.PowerSystem;
using UnityEngine;

namespace PrototypeSubMod.Upgrades;

internal class ProtoUpgradeManager : MonoBehaviour, ISaveDataListener
{
    public Action onInstalledUpgradesChanged;

    private List<TechType> InstalledUpgrades
    {
        get
        {
            if (_installedUpgrades == null || upgradesDirty)
            {
                _installedUpgrades = new();
                foreach (var upgrade in upgrades)
                {
                    if ((upgrade.Value as IProtoUpgrade).GetUpgradeInstalled())
                    {
                        _installedUpgrades.Add(upgrade.Key);
                    }
                }
            }

            return _installedUpgrades;
        }
    }

    private List<TechType> _installedUpgrades;
    private bool upgradesDirty;

    private Dictionary<TechType, ProtoUpgrade> upgrades = new();

    private void Start()
    {
        DevConsole.RegisterConsoleCommand(this, "ToggleUpgradeEnabled");
        DevConsole.RegisterConsoleCommand(this, "ToggleUpgradeInstalled");
        DevConsole.RegisterConsoleCommand(this, "ProtoEndgame");
        
        if (upgrades.Count == 0)
        {
            foreach (var protoUpgrade in GetComponentsInChildren<ProtoUpgrade>(true))
            {
                upgrades.Add(protoUpgrade.techType.TechType, protoUpgrade);
            }
        }
    }

    public void SetUpgradeInstalled(TechType techType, bool installed)
    {
        if (!upgrades.TryGetValue(techType, out var upgrade)) throw new Exception($"There is no upgrade with the tech type {techType} on the Prototype sub");

        (upgrade as IProtoUpgrade).SetUpgradeInstalled(installed);

        upgradesDirty = true;
        onInstalledUpgradesChanged?.Invoke();
    }

    public bool GetUpgradeInstalled(TechType techType)
    {
        return InstalledUpgrades.Contains(techType);
    }

    public void OnSaveDataLoaded(BaseSubDataClass saveData)
    {
        foreach (var protoUpgrade in GetComponentsInChildren<ProtoUpgrade>(true))
        {
            if (upgrades.ContainsKey(protoUpgrade.techType.TechType)) continue;
            
            upgrades.Add(protoUpgrade.techType.TechType, protoUpgrade);
        }

        var protoData = saveData.EnsureAsPrototypeData();
        foreach (var upgrade in upgrades)
        {
            bool installed = protoData.installedModules.Contains(upgrade.Key);

            (upgrade.Value as IProtoUpgrade).SetUpgradeInstalled(installed);
        }
    }

    public void OnBeforeDataSaved(ref BaseSubDataClass saveData)
    {
        var protoData = saveData.EnsureAsPrototypeData();

        upgradesDirty = true;
        protoData.installedModules = InstalledUpgrades;
        saveData = protoData;
    }

    public List<TechType> GetInstalledUpgradeTypes()
    {
        return InstalledUpgrades;
    }

    public List<ProtoUpgrade> GetInstalledUpgrades()
    {
        List<ProtoUpgrade> upgrades = new();
        foreach (var upgrade in this.upgrades)
        {
            upgrades.Add(upgrade.Value);
        }

        return upgrades;
    }

    public void OnConsoleCommand_toggleupgradeenabled(NotificationCenter.Notification notification)
    {
        (bool, TechType) techTypeResult = TryParseTTFromNotification(notification);
        if (!techTypeResult.Item1) return;

        if (!upgrades.TryGetValue(techTypeResult.Item2, out var upgrade))
        {
            ErrorMessage.AddError($"Upgrade with tech type \"{techTypeResult.Item2}\" is not on the prototype");
            return;
        }

        upgrade.SetUpgradeEnabled(!upgrade.GetUpgradeEnabled());

        ErrorMessage.AddError($"{techTypeResult.Item2} enabled set to {upgrade.GetUpgradeEnabled()}");
    }

    public void OnConsoleCommand_toggleupgradeinstalled(NotificationCenter.Notification notification)
    {
        (bool, TechType) techTypeResult = TryParseTTFromNotification(notification);
        if (!techTypeResult.Item1) return;

        if (!upgrades.TryGetValue(techTypeResult.Item2, out var upgrade))
        {
            ErrorMessage.AddError($"Upgrade with tech type \"{techTypeResult.Item2}\" is not on the prototype");
            return;
        }

        upgrade.SetUpgradeInstalled(!upgrade.GetUpgradeInstalled());

        ErrorMessage.AddError($"{techTypeResult.Item2} installed set to {upgrade.GetUpgradeInstalled()}");
        upgradesDirty = true;

        onInstalledUpgradesChanged?.Invoke();
    }

    public void OnConsoleCommand_protoendgame(NotificationCenter.Notification notification)
    {
        foreach (var techType in upgrades.Keys)
        {
            if (upgrades[techType].installedAtStart) continue;
            
            SetUpgradeInstalled(techType, true);
        }

        GetComponentInChildren<ProtoFinsManager>().SetInstalledFinCount(4);
        GetComponentInChildren<PrototypePowerSystem>().SetAllowedSourcesCount(6);
        
        ErrorMessage.AddError("Prototype story progressed to the end: all upgrades installed and final ping spawned");
        
        if (Plugin.GlobalSaveData.storyEndPingSpawned) return;
        
        Plugin.GlobalSaveData.storyEndPingSpawned = true;
        UWE.CoroutineHost.StartCoroutine(SpawnStoryPing());
    }

    private IEnumerator SpawnStoryPing()
    {
        var task = CraftData.GetPrefabForTechTypeAsync(Plugin.StoryEndPingTechType);
        yield return task;

        var prefab = task.GetResult();
        Instantiate(prefab, Plugin.StoryEndPos, Quaternion.identity);
    }

    private (bool, TechType) TryParseTTFromNotification(NotificationCenter.Notification notification)
    {
        if (notification == null)
        {
            ErrorMessage.AddError($"'toggleupgradeinstalled' expects an upgrade tech type.");
            return (false, TechType.None);
        }

        if (notification.data.Count > 1 || notification.data.Count <= 0)
        {
            ErrorMessage.AddError($"Invalid argument count. ToggleUpgradeInstalled expects the tech type for an upgrade");
            return (false, TechType.None);
        }

        string upgradeTechType = "";
        try
        {
            upgradeTechType = notification.data[0] as string;
        }
        catch (Exception e)
        {
            throw e;
        }

        TechType techType = TechType.None;
        try
        {
            techType = (TechType)Enum.Parse(typeof(TechType), upgradeTechType, true);
        }
        catch (Exception e)
        {
            ErrorMessage.AddError($"Error parsing \"{upgradeTechType}\" as a tech type. Check log for full details.");
            throw e;
        }

        return (true, techType);
    }
}
