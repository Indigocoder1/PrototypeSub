using System;
using PrototypeSubMod.Prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Factors;

public class PrecursorSuitChargeUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image chargeBar;
    [SerializeField] private Image backgroundShadow;
    [SerializeField] private Sprite survivalShadow;
    [SerializeField] private Sprite freedomShadow;
    [SerializeField] private float fillAmountMin;
    [SerializeField] private float fillAmountMax;

    private FactorIonManager factorIonManager;
    
    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        UpdateUIVisibility();
        Inventory.main.equipment.onAddItem += OnAddItem;
        Inventory.main.equipment.onRemoveItem += OnRemoveItem;
        GameModeUtils.onGameModeChanged.AddHandler(this, OnGameModeChanged);
        OnGameModeChanged(GameModeUtils.currentGameMode);
    }

    private void OnAddItem(InventoryItem item)
    {
        UpdateUIVisibility();
    }
    
    private void OnRemoveItem(InventoryItem item)
    {
        UpdateUIVisibility();
    }

    private void OnGameModeChanged(GameModeOption option)
    {
        if ((option & GameModeOption.Freedom) != 0 || (option & GameModeOption.Creative) == GameModeOption.Creative)
        {
            backgroundShadow.sprite = freedomShadow;
        }
        else
        {
            backgroundShadow.sprite = survivalShadow;
        }
    }

    private void UpdateUIVisibility()
    {
        var itemInSlot = Inventory.main.equipment.GetItemInSlot("Body");
        bool hasSuit = itemInSlot?.techType == PrecursorSuit.PrefabInfo.TechType;
        canvasGroup.alpha = hasSuit ? 1 : 0;

        if (itemInSlot == null) return;
        
        factorIonManager = itemInSlot.item.GetComponent<FactorIonManager>();
    }

    private void Update()
    {
        if (factorIonManager == null) return;

        chargeBar.fillAmount = Mathf.Lerp(fillAmountMin, fillAmountMax, factorIonManager.GetNormalizedCharge());
    }

    private void OnDestroy()
    {
        Inventory.main.equipment.onAddItem -= OnAddItem;
        Inventory.main.equipment.onRemoveItem -= OnRemoveItem;
    }
}