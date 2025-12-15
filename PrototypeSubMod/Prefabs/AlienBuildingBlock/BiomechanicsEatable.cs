using System.Collections;
using PrototypeSubMod.Factors;
using PrototypeSubMod.Factors.Biomechanics;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.AlienBuildingBlock;

public class BiomechanicsEatable : MonoBehaviour
{
    [SerializeField] private float foodValue = 25f;
    [SerializeField] private float waterValue = 25f;
    [SerializeField] private float healthValue = 25f;
    [SerializeField] private float ionValue = 20f;

    private FactorActivationManager factorActivationManager;
    private bool eatableActive;

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
        eatableActive = true;
    }
    
    private void OnUnequippedFactor(Factor factor)
    {
        if (factorActivationManager.ContainsFactor(typeof(BiomechanicsFactorLogic))) return;
        
        if (!TryGetComponent(out Eatable eatable)) return;

        Destroy(eatable);
        eatableActive = false;
    }

    public void OnEat()
    {
        UWE.CoroutineHost.StartCoroutine(RefundWarperRemnant());
        Inventory.main.equipment.GetItemInSlot("Body").item.GetComponent<FactorIonManager>().AddEnergy(ionValue);
        if (eatableActive)
        {
            Player.main.liveMixin.AddHealth(healthValue);
        }
        else
        {
            // This is normally called by the eatable when being eaten, but since this can be eaten without that component I'm
            // manually calling it here
            Destroy(gameObject);
        }
    }

    public bool EatableActive() => eatableActive;
    public float GetIonCharge() => ionValue;
    public float GetHealthValue() => healthValue;

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