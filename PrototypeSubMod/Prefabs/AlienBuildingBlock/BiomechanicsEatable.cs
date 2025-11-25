using System.Collections;
using PrototypeSubMod.Factors;
using PrototypeSubMod.Factors.Biomechanics;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.AlienBuildingBlock;

public class BiomechanicsEatable : MonoBehaviour
{
    [SerializeField] private float foodValue = 25f;
    [SerializeField] private float waterValue = 25f;
    [SerializeField] private float ionValue = 20f;

    private FactorActivationManager factorActivationManager;

    private void Awake()
    {
        FactorActivationManager.onEquippedFactor += OnEquippedFactor;
        FactorActivationManager.onUnequippedFactor += OnUnequippedFactor;
        
        factorActivationManager = Player.main.GetComponent<FactorActivationManager>();
        OnEquippedFactor(null);
        OnUnequippedFactor(null);
    }
    
    private void OnEquippedFactor(Factor factor)
    {
        if (!factorActivationManager.ContainsFactor(typeof(BiomechanicsFactorLogic))) return;

        var eatable = gameObject.EnsureComponent<Eatable>();
        eatable.foodValue = foodValue;
        eatable.waterValue = waterValue;
    }
    
    private void OnUnequippedFactor(Factor factor)
    {
        if (factorActivationManager.ContainsFactor(typeof(BiomechanicsFactorLogic))) return;
        
        if (!TryGetComponent(out Eatable eatable)) return;

        Destroy(eatable);
    }

    public void OnEat()
    {
        UWE.CoroutineHost.StartCoroutine(RefundWarperRemnant());
        Inventory.main.equipment.GetItemInSlot("Body").item.GetComponent<FactorIonManager>().AddEnergy(ionValue);
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
        FactorActivationManager.onEquippedFactor -= OnEquippedFactor;
        FactorActivationManager.onUnequippedFactor -= OnUnequippedFactor;
    }
}