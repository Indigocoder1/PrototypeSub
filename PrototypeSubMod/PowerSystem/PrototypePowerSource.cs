using PrototypeSubMod.SaveData;
using SubLibrary.SaveData;
using System.Collections;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.PowerSystem;

public class PrototypePowerSource : MonoBehaviour, IPowerInterface, ISaveDataListener
{
    public float Charge01 => Charge / Capacity;

    public float Charge
    {
        get
        {
            float num = battery != null ? battery.charge : 0;
            return num;
        }
    }

    public float Capacity
    {
        get
        {
            if (battery != null) return battery.capacity;

            return 0;
        }
    }

    [SerializeField] private TechType defaultBattery;
    [SerializeField, Range(0, 1)] private float defaultBatteryCharge;
    [SerializeField] private ChildObjectIdentifier storageRoot;
    [SerializeField] private PrototypePowerSystem powerSystem;

    private PrototypeSaveData protoSaveData;
    private PrototypeSaveData.PowerSourceData powerSourceData;

    private PrototypePowerBattery battery;

    private float enableElectronicsTime;
    private bool _electronicsDisabled;

    private void Start()
    {
        UpdateConnection();

        if (protoSaveData == null && !powerSourceData.defaultBatteryCreated)
        {
            CoroutineHost.StartCoroutine(SpawnDefaultBattery());
        }
    }

    #region IPowerInterface

    public bool GetInboundHasSource(IPowerInterface powerInterface)
    {
        //We don't have any inbound power sources
        return false;
    }

    public float GetMaxPower()
    {
        return Capacity;
    }

    public float GetPower()
    {
        return Charge;
    }

    public bool HasInboundPower(IPowerInterface powerInterface)
    {
        return false;
    }

    public bool ModifyPower(float amount, out float modified)
    {
        float chargeChange;
        modified = 0;

        if (!GameModeUtils.RequiresPower()) return true;
        
        if (battery == null) return false;
        
        if (amount >= 0)
        {
            chargeChange = Mathf.Min(amount, Capacity - Charge);
        }
        else
        {
            chargeChange = GetChargeChangeSubtract(amount);
        }
        
        modified = chargeChange;
        if (Charge + chargeChange > Capacity || Charge + chargeChange < 0)
        {
            float min = Mathf.Min(Capacity - Charge, Charge);
            float max = Mathf.Max(Capacity - Charge, Charge);
            modified = Mathf.Clamp(modified, min, max);
        }
        
        // Returns whether the amount drawn was less than the charge in the battery
        // I.e. returns false if a power draw of 400 is requested when we have 200 charge
        bool returnCondition = amount >= 0f ? amount <= Capacity - Charge : Charge > -amount;
        battery.ModifyCharge(chargeChange);
        
        return returnCondition;
    }

    private float GetChargeChangeSubtract(float amount)
    {
        float chargeChange;
        
        int incrementCount = Mathf.FloorToInt(amount / PrototypePowerSystem.CHARGE_POWER_AMOUNT);
        float chargeRemainder = -(amount / PrototypePowerSystem.CHARGE_POWER_AMOUNT - incrementCount) * PrototypePowerSystem.CHARGE_POWER_AMOUNT;
        
        float mod = Charge % PrototypePowerSystem.CHARGE_POWER_AMOUNT;
        
        if (Charge + amount > 0)
        {
            bool zeroCheck = mod != 0;
            
            // I wrote this yesterday and barely remember how it works
            // Don't touch it if possible
            float deltaToNextCharge = zeroCheck 
                ? Mathf.Max(-(chargeRemainder + PrototypePowerSystem.CHARGE_POWER_AMOUNT), -mod)
                : -Mathf.Min(-amount, PrototypePowerSystem.CHARGE_POWER_AMOUNT);
            float additiveAmount = -amount > PrototypePowerSystem.CHARGE_POWER_AMOUNT ? amount : 0;
            float powerOffset = -amount > PrototypePowerSystem.CHARGE_POWER_AMOUNT
                ? PrototypePowerSystem.CHARGE_POWER_AMOUNT
                : 0;
            chargeChange = additiveAmount + deltaToNextCharge + powerOffset;
        }
        else
        {
            chargeChange = -Mathf.Min(-amount, Charge);
        }
        
        return chargeChange;
    }

    #endregion

    private void UpdateConnection()
    {
        var relay = PowerSource.FindRelay(transform);
        relay.AddInboundPower(this);
    }

    public void OnSaveDataLoaded(BaseSubDataClass saveData)
    {
        protoSaveData = saveData.EnsureAsPrototypeData();

        if (protoSaveData.powerSourceDatas == null)
        {
            protoSaveData.powerSourceDatas = new();
        }

        if (!protoSaveData.powerSourceDatas.ContainsKey(gameObject.name))
        {
            protoSaveData.powerSourceDatas.Add(gameObject.name, new PrototypeSaveData.PowerSourceData());
        }

        powerSourceData = protoSaveData.powerSourceDatas[gameObject.name];
        if (!powerSourceData.defaultBatteryCreated)
        {
            CoroutineHost.StartCoroutine(SpawnDefaultBattery());
        }
    }

    public void OnBeforeDataSaved(ref BaseSubDataClass saveData)
    {
        var protoData = saveData.EnsureAsPrototypeData();

        protoData.powerSourceDatas[gameObject.name] = powerSourceData;
    }

    public IEnumerator SpawnDefaultBattery()
    {
        if (this.battery) yield break;
        if (defaultBattery == TechType.None) yield break;
        
        CoroutineTask<GameObject> prefabTask = CraftData.GetPrefabForTechTypeAsync(defaultBattery);

        yield return prefabTask;

        GameObject prefab = prefabTask.result.Get();
        var instantiatedPrefab = Instantiate(prefab, storageRoot.transform);
        instantiatedPrefab.SetActive(false);

        var battery = instantiatedPrefab.GetComponent<PrototypePowerBattery>();
        this.battery = battery;

        battery.SetChargeNormalized(defaultBatteryCharge);

        string slot = PrototypePowerSystem.SLOT_NAMES[transform.GetSiblingIndex()];

        powerSystem.equipment.AddItem(slot, battery.InventoryItem);
        
        powerSourceData.defaultBatteryCreated = true;
    }

    public void SetBattery(PrototypePowerBattery battery)
    {
        this.battery = battery;

        string slot = PrototypePowerSystem.SLOT_NAMES[transform.GetSiblingIndex()];

        if (battery == null)
        {
            powerSystem.equipment.RemoveItem(slot, false, false);
        }
        else
        {
            battery.Initialize();
            powerSystem.equipment.AddItem(slot, battery.InventoryItem);
        }
    }

    public int GetRemainingCharges()
    {
        return Mathf.CeilToInt(Charge / PrototypePowerSystem.CHARGE_POWER_AMOUNT);
    }

    public float GetCurrentChargePower01()
    {
        float mod = Charge % PrototypePowerSystem.CHARGE_POWER_AMOUNT;
        return mod == 0 ? 1 : mod / PrototypePowerSystem.CHARGE_POWER_AMOUNT;
    }

    public bool HasBattery() => battery != null;
}
