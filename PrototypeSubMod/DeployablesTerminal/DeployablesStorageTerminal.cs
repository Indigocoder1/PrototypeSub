using PrototypeSubMod.SaveData;
using SubLibrary.SaveData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrototypeSubMod.DeployablesTerminal;

internal class DeployablesStorageTerminal : MonoBehaviour, ISaveDataListener, ILateSaveDataListener
{
    public const string PHASE_GATE_SLOT = "PhaseGateSlot1";

    public static Vector3[] SLOT_POSITIONS { get; } = {
        new(-166, -98, 0),
        new(28, 130, 0),
        new(-56, -212, 0),
        new(138.5f, 17.5f, 0),
    };

    public static Vector3 PHASE_GATE_SLOT_POS { get; } = new(0, 200, 0);

    public static string[] LightBeaconSlots { get; } = new[]
    {
        "DeployableStorageSlot1",
        "DeployableStorageSlot2",
        "DeployableStorageSlot3",
        "DeployableStorageSlot4"
    };

    private static bool SlotmappingInitialized;

    public Equipment equipment { get; private set; }

    [SerializeField] private GameObject storageRoot;
    [SerializeField] private FMODAsset equipSound;
    [SerializeField] private FMODAsset unequipSound;
    [SerializeField] private ProtoDeployableManager deployableManager;

    private bool ignoreSoundNextEquip;

    private void Awake()
    {
        Initialize();
    }

    public void OnHover(HandTargetEventData eventData)
    {
        if (Plugin.GlobalSaveData.prototypeDestroyed) return;

        HandReticle main = HandReticle.main;
        main.SetText(HandReticle.TextType.Hand, "UseDeployableTerminal", true, GameInput.Button.LeftHand);
        main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false, GameInput.Button.None);
        main.SetIcon(HandReticle.IconType.Hand, 1f);
    }

    public void OnUse(HandTargetEventData eventData)
    {
        if (Plugin.GlobalSaveData.prototypeDestroyed) return;

        PDA pda = Player.main.GetPDA();
        Inventory.main.SetUsedStorage(equipment);
        pda.Open(PDATab.Inventory);
    }

    private void Initialize()
    {
        if (equipment != null) return;

        InitializeSlotMapping();

        equipment = new(gameObject, storageRoot.transform);
        equipment.SetLabel("ProtoDeployableEquipmentLabel");
        equipment.onEquip += OnEquip;
        equipment.onUnequip += OnUnequip;
 
        equipment.typeToSlots = new Dictionary<EquipmentType, List<string>>
        {
            { Plugin.LightBeaconEquipmentType, LightBeaconSlots.ToList() },
            { Plugin.PhaseGateEquipmentType, new List<string> { PHASE_GATE_SLOT } }
        };
        
        var slots = new List<string>();
        slots.AddRange(LightBeaconSlots);
        slots.Add(PHASE_GATE_SLOT);
        equipment.AddSlots(slots);
    }

    private void InitializeSlotMapping()
    {
        if (SlotmappingInitialized) return;

        foreach (string slot in LightBeaconSlots)
        {
            Equipment.slotMapping.Add(slot, Plugin.LightBeaconEquipmentType);
        }

        Equipment.slotMapping.Add(PHASE_GATE_SLOT, Plugin.PhaseGateEquipmentType);

        SlotmappingInitialized = true;
    }

    private void OnEquip(string slot, InventoryItem item)
    {
        if (equipSound != null && !ignoreSoundNextEquip)
        {
            FMODUWE.PlayOneShot(equipSound, transform.position, 2f);
        }

        deployableManager.RecalculateDeployableTotals();
        ignoreSoundNextEquip = false;
    }

    private void OnUnequip(string slot, InventoryItem item)
    {
        if (unequipSound != null)
        {
            FMODUWE.PlayOneShot(unequipSound, transform.position, 2f);
        }

        deployableManager.RecalculateDeployableTotals();
    }

    public void OnSaveDataLoaded(BaseSubDataClass saveData)
    {
        Initialize();
    }

    public void OnBeforeDataSaved(ref BaseSubDataClass saveData)
    {
        var protoData = saveData.EnsureAsPrototypeData();
        protoData.serializedDeployablesEquipment = equipment.SaveEquipment();

        saveData = protoData;
    }

    public void OnLateSaveDataLoaded(BaseSubDataClass saveData)
    {
        var data = saveData.EnsureAsPrototypeData();
        if (data.serializedDeployablesEquipment != null)
        {
            StorageHelper.TransferEquipment(storageRoot.gameObject, data.serializedDeployablesEquipment, equipment);
        }
    }

    public void IgnoreSoundNextEquip()
    {
        ignoreSoundNextEquip = true;
    }
}
