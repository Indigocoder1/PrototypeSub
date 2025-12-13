using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Factors.Blink;

public class BlinkResourceUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image resourceBar;
    [SerializeField] private float fillAmountMin;
    [SerializeField] private float fillAmountMax;

    private FactorActivationManager factorActivationManager;
    
    private void Start()
    {
        factorActivationManager = Player.main.GetComponent<FactorActivationManager>();
        
        UpdateUIVisibility();
        Inventory.main.equipment.onAddItem += OnAddItem;
        Inventory.main.equipment.onRemoveItem += OnRemoveItem;
    }

    private void OnAddItem(InventoryItem item)
    {
        UpdateUIVisibility();
    }
    
    private void OnRemoveItem(InventoryItem item)
    {
        UpdateUIVisibility();
    }

    private void UpdateUIVisibility()
    {
        bool hasBlink = factorActivationManager.ContainsFactor(typeof(Blink));
        canvasGroup.alpha = hasBlink ? 1 : 0;
    }

    public void SetFillAmount(float amount)
    {
        resourceBar.fillAmount = Mathf.Lerp(fillAmountMin, fillAmountMax, amount);
    }
    
    private void OnDestroy()
    {
        Inventory.main.equipment.onAddItem -= OnAddItem;
        Inventory.main.equipment.onRemoveItem -= OnRemoveItem;
    }
}