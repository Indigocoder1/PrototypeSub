using System;
using PrototypeSubMod.Prefabs;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Interceptor;

public class ReactorRadiationManager : MonoBehaviour
{
    [SerializeField] private RadiatePlayerInRange radiatePlayerInRange;
    [SerializeField] private DamagePlayerInRadius damagePlayerInRadius;

    private float originalDamage;
    
    private void Start()
    {
        Inventory.main.equipment.onEquip += OnEquipmentChanged;
        Inventory.main.equipment.onUnequip += OnEquipmentChanged;
        originalDamage = damagePlayerInRadius.damageAmount;
        OnEquipmentChanged(string.Empty, null);
    }

    private void OnEquipmentChanged(string slot, InventoryItem item)
    {
        var hasPrecursorSuit = Inventory.main.equipment.GetCount(PrecursorSuit.prefabInfo.TechType) > 0;
        radiatePlayerInRange.enabled = !hasPrecursorSuit;
        damagePlayerInRadius.damageAmount = hasPrecursorSuit ? 0 : originalDamage;
    }

    private void OnDestroy()
    {
        Inventory.main.equipment.onEquip -= OnEquipmentChanged;
        Inventory.main.equipment.onUnequip -= OnEquipmentChanged;
    }
}