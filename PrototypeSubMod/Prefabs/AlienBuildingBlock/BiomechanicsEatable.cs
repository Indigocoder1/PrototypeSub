using System;
using System.Collections;
using PrototypeSubMod.Factors;
using PrototypeSubMod.Factors.Biomechanics;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.AlienBuildingBlock;

public class BiomechanicsEatable : MonoBehaviour
{
    [SerializeField] private float foodValue = 25f;
    [SerializeField] private float waterValue = 25f;

    private FactorManager factorManager;

    private void Awake()
    {
        FactorManager.onEquippedFactor += OnEquippedFactor;
        FactorManager.onUnequippedFactor += OnUnequippedFactor;
        
        factorManager = Player.main.GetComponent<FactorManager>();
        OnEquippedFactor(null);
    }
    
    private void OnEquippedFactor(Factor factor)
    {
        if (!factorManager.ContainsFactor(typeof(BiomechanicsFactorLogic))) return;

        var eatable = gameObject.EnsureComponent<Eatable>();
        eatable.foodValue = foodValue;
        eatable.waterValue = waterValue;
    }
    
    private void OnUnequippedFactor(Factor factor)
    {
        if (factorManager.ContainsFactor(typeof(BiomechanicsFactorLogic))) return;
        
        if (!TryGetComponent(out Eatable eatable)) return;

        Destroy(eatable);
    }

    public void OnEat()
    {
        UWE.CoroutineHost.StartCoroutine(RefundWarperRemnant());
    }

    private IEnumerator RefundWarperRemnant()
    {
        var prefabTask = CraftData.GetPrefabForTechTypeAsync(WarperRemnant.prefabInfo.TechType);
        yield return prefabTask;

        var prefab = prefabTask.GetResult();
        var pickupable = Instantiate(prefab).GetComponent<Pickupable>();
        Inventory.main.ForcePickup(pickupable);
    }

    private void OnDestroy()
    {
        FactorManager.onEquippedFactor -= OnEquippedFactor;
        FactorManager.onUnequippedFactor -= OnUnequippedFactor;
    }
}