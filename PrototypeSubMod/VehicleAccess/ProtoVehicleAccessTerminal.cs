using System;
using System.Collections.Generic;
using System.Linq;
using PrototypeSubMod.Docking;
using PrototypeSubMod.StructureLoading;
using UnityEngine;

namespace PrototypeSubMod.VehicleAccess;

public class ProtoVehicleAccessTerminal : MonoBehaviour
{
    private static readonly int ProxyActivated = Animator.StringToHash("ProxyActivated");
    public const string SLOT_NAME = "VehicleAccessPlaceholderSlot";
    public const string EQUIPMENT_LABEL = "VehicleAccessEquipment";
    
    public Equipment equipment { get; private set; }

    [SerializeField] private Animator animator;
    [SerializeField] private VehicleDockingBay dockingBay;

    private ProtoVehicleAccessManager accessManager;
    private ProtoDockingManager dockingManager;
    private uGUI_InventoryTab inventoryTab;
    
    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (equipment != null) return;

        dockingManager = dockingBay.GetComponentInChildren<ProtoDockingManager>(true);
        equipment = new(gameObject, transform);
        equipment.SetLabel(EQUIPMENT_LABEL);
        equipment.AddSlots(new[] { SLOT_NAME });

        equipment.typeToSlots = new Dictionary<EquipmentType, List<string>> { { EquipmentType.None, Array.Empty<string>().ToList() } };

        accessManager = uGUI_PDA.main.GetComponentInChildren<ProtoVehicleAccessManager>(true);
        inventoryTab = uGUI_PDA.main.GetComponentInChildren<uGUI_InventoryTab>(true);
        accessManager.SetInventoryTab(inventoryTab);
    }
    
    public void OnHover(HandTargetEventData eventData)
    {
        string key = string.Empty;

        var handReticle = HandReticle.IconType.Hand;
        if (dockingBay.dockedVehicle)
        {
            key = "ProtoAccessVehicle";
        }
        else if (!dockingManager.DockUnlocked())
        {
            key = "ProtoDockNotUnlocked";
            handReticle = HandReticle.IconType.HandDeny;
        }
        else
        {
            key = "ProtoNoVehicleDocked";
            handReticle = HandReticle.IconType.HandDeny;
        }
        
        var main = HandReticle.main;
        main.SetText(HandReticle.TextType.Hand, key, true, GameInput.Button.LeftHand);
        main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
        main.SetIcon(handReticle, 1f);
    }

    public void OnUse(HandTargetEventData eventData)
    {
        if (!dockingBay.dockedVehicle) return;

        inventoryTab.usedStorageGrids.Clear();
        Inventory.main.SetUsedStorage(equipment);
        Player.main.pda.Open(PDATab.Inventory);
        inventoryTab.usedStorageGrids.Insert(0, accessManager);

        accessManager.SetTerminal(this);
    }

    public bool OpenStorage(PDA.OnClose onClosePda = null)
    {
        var storageInputs = dockingBay.dockedVehicle.GetComponentsInChildren<SeamothStorageInput>();
        var storageContainers = dockingBay.dockedVehicle.GetComponentsInChildren<StorageContainer>();

        if ((storageContainers == null || storageContainers.Length == 0) &&
            (storageInputs == null || storageInputs.Length == 0)) return false;
        
        var pda = Player.main.pda;
        pda.ui.OnClosePDA();
        Inventory.main.ClearUsedStorage();
        pda.onCloseCallback = null;
        
        foreach (var storageInput in storageInputs)
        {
            var storageInSlot = storageInput.seamoth.GetStorageInSlot(storageInput.slotID, TechType.VehicleStorageModule);
            Inventory.main.SetUsedStorage(storageInSlot, true);
        }
        
        foreach (var storageContainer in storageContainers)
        {
            Inventory.main.SetUsedStorage(storageContainer.container, true);
            pda.onCloseCallback += storageContainer.OnClosePDA;
        }
        
        if (onClosePda != null)
        {
            pda.onCloseCallback += onClosePda;
        }
        pda.ui.OnOpenPDA(PDATab.Inventory);
        pda.ui.Select();
        pda.ui.OnPDAOpened();

        return true;
    }

    public void OpenUpgrades(PDA.OnClose onClosePda = null)
    {
        var upgradeConsoleInput = dockingBay.dockedVehicle.GetComponentInChildren<VehicleUpgradeConsoleInput>();
        if (!upgradeConsoleInput) return;
        
        var pda = Player.main.pda;
        pda.ui.OnClosePDA();
        pda.onCloseCallback = null;
        
        Inventory.main.SetUsedStorage(upgradeConsoleInput.equipment);
        pda.ui.OnOpenPDA(PDATab.Inventory);
        pda.ui.Select();
        pda.ui.OnPDAOpened();
        
        if (onClosePda != null)
        {
            pda.onCloseCallback += onClosePda;
        }
    }

    public void QuickOpenManager(PDA.OnClose onClosePda = null)
    {
        var pda = Player.main.pda;
        pda.ui.OnClosePDA();
        pda.onCloseCallback = null;
        
        inventoryTab.usedStorageGrids.Clear();
        Inventory.main.SetUsedStorage(equipment);

        accessManager.SetTerminal(this);

        if (onClosePda != null)
        {
            pda.onCloseCallback += onClosePda;
        }
        pda.ui.OnOpenPDA(PDATab.Inventory);
        pda.ui.Select();
        pda.ui.OnPDAOpened();
        
        inventoryTab.usedStorageGrids.Insert(0, accessManager);
    }

    public void OnPlayerEnterRange()
    {
        if (!dockingBay.dockedVehicle) return;
        
        animator.SetBool(ProxyActivated, true);
    }

    public void OnPlayerExitRange()
    {
        animator.SetBool(ProxyActivated, false);
    }
}