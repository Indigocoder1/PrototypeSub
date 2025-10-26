using PrototypeSubMod.SaveData;
using SubLibrary.SaveData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.PowerSystem;

public class PrototypePowerSystem : MonoBehaviour, ISaveDataListener, IProtoTreeEventListener
{
    internal static readonly string[] SLOT_NAMES = {
        "PrototypePowerSlot1",
        "PrototypePowerSlot2",
        "PrototypePowerSlot3",
        "PrototypePowerSlot4",
        "PrototypePowerSlot5",
        "PrototypePowerSlot6"
    };

    internal static Dictionary<TechType, PowerConfigData> AllowedPowerSources;

    public const float CHARGE_POWER_AMOUNT = 200f;

    internal static readonly string EquipmentLabel = "PrototypePowerLabel";

    public Equipment equipment { get; private set; }
    public event Action onReorderSources;
    public event Action onAllowedSourcesChanged;

    [SerializeField] private SubSerializationManager serializationManager;
    [SerializeField] private ChildObjectIdentifier storageRoot;
    [SerializeField] private PrototypePowerSource[] batterySources;
    [SerializeField] private ProtoPowerRelay[] powerRelays;
    [SerializeField] private FMOD_CustomLoopingEmitter ambientSFX;

    private Coroutine relayActivatorCoroutine;
    private bool reorderingItems;
    private int allowedPowerSourceCount = 2;
    
    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        if (batterySources.Length != SLOT_NAMES.Length)
        {
            Plugin.Logger.LogError($"Battery source and slot name length mismatch on {gameObject}!");
        }

        UpdateActiveRelays();
        UpdateAmbientSFX();
    }

    private void Initialize()
    {
        if (equipment != null) return;

        equipment = new(gameObject, storageRoot.transform);
        equipment.SetLabel(EquipmentLabel);
        equipment.onEquip += OnEquip;
        equipment.onUnequip += OnUnequip;
        equipment.onRemoveItem += OnRemoveItem;

        Plugin.Logger.LogInfo($"Adding slots. Len = {equipment.equipment.Count}");
        Plugin.Logger.LogInfo($"Slot mapping = {Equipment.slotMapping}");
        equipment.AddSlots(SLOT_NAMES);
        Plugin.Logger.LogInfo($"Added slots. Len = {equipment.equipment.Count}");
        
        equipment.isAllowedToAdd = IsAllowedToAdd;
        equipment.isAllowedToRemove = (p, v) => true;
        
        equipment.typeToSlots = new()
        {
            { Plugin.DummyPowerType, SLOT_NAMES.ToList() }
        };
        
        foreach (var relay in powerRelays)
        {
            relay.SetRelayActive(false);
        }
    }
    
    private void OnEquip(string slot, InventoryItem item)
    {
        int index = Array.IndexOf(SLOT_NAMES, slot);

        var batterySource = batterySources[index];

        if (!item.item.TryGetComponent(out PrototypePowerBattery battery))
        {
            Plugin.Logger.LogError($"Item ({item}) added to prototype power system doesn't have a PrototypePowerBattery component on it!");
            return;
        }

        batterySource.SetBattery(battery);
        UpdateRelayStatus();
        UpdateAmbientSFX();
    }

    private void OnUnequip(string slot, InventoryItem item)
    {
        int index = Array.IndexOf(SLOT_NAMES, slot);

        var batterySource = batterySources[index];
        batterySource.SetBattery(null);
        UpdateAmbientSFX();
    }

    private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
    {
        return AllowedPowerSources.Keys.Contains(pickupable.GetTechType());
    }

    public void OnSaveDataLoaded(BaseSubDataClass saveData)
    {
        Initialize();
        allowedPowerSourceCount = saveData.EnsureAsPrototypeData().allowedPowerSourceCount;
    }

    public void OnBeforeDataSaved(ref BaseSubDataClass saveData)
    {
        var protoData = saveData.EnsureAsPrototypeData();
        protoData.serializedPowerEquipment = equipment.SaveEquipment();
        protoData.allowedPowerSourceCount = allowedPowerSourceCount;
        
        saveData = protoData;
    }

    public void OnProtoSerializeObjectTree(ProtobufSerializer serializer) { }

    public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
    {
        Initialize();
        UWE.CoroutineHost.StartCoroutine(OnDeserialized());
    }

    private IEnumerator OnDeserialized()
    {
        yield return new WaitForEndOfFrame();
        
        Initialize();
        
        var data = serializationManager.saveData.EnsureAsPrototypeData();
        if (data.serializedPowerEquipment != null)
        {
            foreach (var kvp in data.serializedPowerEquipment)
            {
                Plugin.Logger.LogInfo($"{kvp.Key} | {kvp.Value}");
            }
            StorageHelper.TransferEquipment(storageRoot.gameObject, data.serializedPowerEquipment, equipment);
        }

        UpdateRelayStatus();
    }

    public static void AddPowerSource(TechType techType, PowerConfigData configData)
    {
        if (AllowedPowerSources.ContainsKey(techType)) return;

        AllowedPowerSources.Add(techType, configData);
    }

    public bool StorageSlotsFull()
    {
        return storageRoot.transform.childCount >= allowedPowerSourceCount;
    }
    
    public int GetInstalledSourceCount() => storageRoot.transform.childCount;
    public int GetAllowedSourcesCount() => allowedPowerSourceCount;
    
    public void SetAllowedSourcesCount(int count)
    {
        allowedPowerSourceCount = count;
        UpdateActiveRelays();
        onAllowedSourcesChanged?.Invoke();
    }

    private void UpdateActiveRelays()
    {
        for (int i = 0; i < powerRelays.Length; i++)
        {
            bool active = i + 1 <= allowedPowerSourceCount;
            powerRelays[i].gameObject.SetActive(active);
        }
    }

    private void HandleFallbackPowerIssues()
    {
        if (equipment.equipment.Count < SLOT_NAMES.Length)
        {
            foreach (var slot in SLOT_NAMES)
            {
                if (equipment.equipment.ContainsKey(slot)) continue;
                
                equipment.equipment.Add(slot, null);
            }
        }

        if (allowedPowerSourceCount < storageRoot.transform.childCount)
        {
            SetAllowedSourcesCount(storageRoot.transform.childCount);
        }
    }

    public PrototypePowerSource[] GetPowerSources() => batterySources;
    
    private void OnRemoveItem(InventoryItem item)
    {
        if (reorderingItems) return;
        
        UWE.CoroutineHost.StartCoroutine(OnRemoveItemDelayed());
    }

    private IEnumerator OnRemoveItemDelayed()
    {
        reorderingItems = true;
        yield return new WaitForEndOfFrame();
        
        // Items will only be removed via the consumption of an item, or the removal of one
        List<InventoryItem> newItems = new();
        for (int i = 0; i < SLOT_NAMES.Length; i++)
        {
            string slot = SLOT_NAMES[i];
            var itemInSlot = equipment.GetItemInSlot(slot);
            if (itemInSlot == null) continue;

            newItems.Add(itemInSlot);
            equipment.RemoveItem(slot, false, false);
        }

        for (int i = 0; i < newItems.Count; i++)
        {
            equipment.AddItem(SLOT_NAMES[i], newItems[i]);
        }

        UpdateRelayStatus();
        UpdateAmbientSFX();
        reorderingItems = false;

        onReorderSources?.Invoke();
    }

    public void UpdateRelayStatus()
    {
        List<ProtoPowerRelay> activeRelays = new();
        
        for (int i = 0; i < powerRelays.Length; i++)
        {
            if (i >= SLOT_NAMES.Length) break;
            
            var relay = powerRelays[i];
            var itemInSlot = equipment.GetItemInSlot(SLOT_NAMES[i]);
            relay.SetPowerSource(itemInSlot);
            bool active = itemInSlot != null;
            if (active)
            {
                activeRelays.Add(relay);
            }
            else
            {
                relay.SetRelayActive(false);
            }
        }

        if (relayActivatorCoroutine != null) UWE.CoroutineHost.StopCoroutine(relayActivatorCoroutine);
        relayActivatorCoroutine = UWE.CoroutineHost.StartCoroutine(UpdateActiveRelays(activeRelays));
    }

    private IEnumerator UpdateActiveRelays(List<ProtoPowerRelay> relays)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < relays.Count; i++)
        {
            var relay = relays[i];
            relay.SetRelayActive(true);
            
            var nextRelay = relays[Mathf.Min(i + 1, relays.Count - 1)];
            if (!nextRelay.GetRelayActive())
            {
                yield return new WaitForSeconds(Random.Range(0f, 1f));
            }
        }
    }

    private void UpdateAmbientSFX()
    {
        bool active = equipment.equipment.Values.Any(i => i != null);
        if (active)
        {
            ambientSFX.Play();
        }
        else
        {
            ambientSFX.Stop();
        }
    }

    private void OnDisable()
    {
        ambientSFX.Stop(STOP_MODE.IMMEDIATE);
    }

    private void OnEnable()
    {
        UpdateAmbientSFX();
    }
}
