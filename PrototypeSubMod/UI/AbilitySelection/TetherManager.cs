using PrototypeSubMod.Patches;
using PrototypeSubMod.UI.ProceduralArcGenerator;
using SubLibrary.UI;
using System;
using System.Collections.Generic;
using PrototypeSubMod.PrototypeStory;
using PrototypeSubMod.UI.ActivatedAbilities;
using PrototypeSubMod.Utility;
using Story;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.UI.AbilitySelection;

public class TetherManager : MonoBehaviour, IUIElement
{
    public Action<IAbilityIcon> onAbilityActivatedChanged;
    public Action onAbilitySelected;

    [SerializeField] private ActivatedAbilitiesManager activatedAbilitiesManager;
    [SerializeField] private VoiceNotificationManager notificationManager;
    [SerializeField] private VoiceNotification invalidOperationVoiceline;
    [SerializeField] private ProtoUpgradeCategory[] upgradeCategories;
    [SerializeField] private PilotingChair chair;
    [SerializeField] private Transform tetherPoint;
    [SerializeField] private CircularMeshApplier selectionHighlight;
    [SerializeField] private IconDistributor distributor;
    [SerializeField] private Image selectionPreview;
    [SerializeField] private FMODAsset genericAbilitySFX;
    [SerializeField] private FMODAsset openSFX;
    [SerializeField] private FMODAsset hoverSFX;
    [SerializeField] private FMODAsset selectSFX;
    [SerializeField] private float minTimeBetweenActivationSFX;
    [SerializeField] private float tetherSensitivity;
    [SerializeField] private float tetherLength;

    private RadialIcon lastIcon;
    private RadialIcon selectedIcon;
    private Dictionary<TechType, ProtoUpgradeCategory> typeToCategory;
    private Dictionary<ProtoUpgradeCategory, VoiceNotification> categoryNotifications;
    private VoiceNotification genericAbilityNofif;

    private float lastTetherAngle;
    private float timeLastAngleCalculated;
    private bool menuOpen;
    private float timeOpened;

    private void Start()
    {
        typeToCategory = new Dictionary<TechType, ProtoUpgradeCategory>();
        categoryNotifications = new Dictionary<ProtoUpgradeCategory, VoiceNotification>();
        genericAbilityNofif = gameObject.AddComponent<VoiceNotification>();
        genericAbilityNofif.sound = genericAbilitySFX;
        genericAbilityNofif.minInterval = minTimeBetweenActivationSFX;
        foreach (var category in upgradeCategories)
        {
            foreach (var type in category.ownedTechTypes)
            {
                typeToCategory.Add(type.TechType, category);
            }

            var notif = gameObject.AddComponent<VoiceNotification>();
            notif.sound = category.activationSFX;
            notif.minInterval = minTimeBetweenActivationSFX;
            categoryNotifications.Add(category, notif);
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        selectionHighlight.gameObject.SetActive(false);
        selectionHighlight.enabled = true;

        RegenerateHighlightArc();
    }

    public void UpdateUI()
    {
        if (Time.time - 0.01f <= timeOpened) return;

        if (selectedIcon == null)
        {
            GetComponentInParent<SelectionMenuManager>().SelectDefaultIcon();
        }
        
        HandleActivation();

        if (!menuOpen) return;

        UpdateTetherPoint();
        UpdateIconNotifs();
        UpdateSelection();
    }

    public void OnSubDestroyed() { }

    private void UpdateTetherPoint()
    {
        Vector2 delta = MainCameraControl_Patches.GetOverwrittenLookDelta() * Time.deltaTime * tetherSensitivity;
        tetherPoint.localPosition += (Vector3)delta;

        tetherPoint.localPosition = tetherPoint.localPosition.normalized * tetherLength;
        tetherPoint.localEulerAngles = new Vector3(0, 0, CalculateTetherAngle());
    }

    private void UpdateIconNotifs()
    {
        var currentIcon = GetIconClosestToPointer();

        if (currentIcon != lastIcon)
        {
            if (lastIcon && lastIcon.GetHovering())
            {
                lastIcon.OnTetherExit();
            }

            if (currentIcon && !currentIcon.GetHovering())
            {
                currentIcon.OnTetherEnter();
            }

            FMODUWE.PlayOneShot(hoverSFX, transform.position, 0.1f);
        }

        if (!currentIcon)
        {
            selectionHighlight.gameObject.SetActive(false);
        }
        else if (currentIcon.GetHovering())
        {
            selectionHighlight.gameObject.SetActive(true);
            float increment = distributor.GetIncrement();
            selectionHighlight.transform.localEulerAngles = new Vector3(0, 0, currentIcon.transform.GetSiblingIndex() * increment - increment / 2);
        }

        lastIcon = currentIcon;
    }

    private void UpdateSelection()
    {
        if (!GameInput.GetButtonDown(GameInput.Button.LeftHand)) return;

        if (!lastIcon) return;

        if (selectedIcon) selectedIcon.Deselect();

        SelectIcon(lastIcon);
    }

    public void SelectIcon(RadialIcon icon, bool forceColSwap = false, bool playSFX = true)
    {
        icon.Select();
        selectedIcon = icon;
        selectionPreview.sprite = icon.GetAbility().GetSprite();
        onAbilitySelected?.Invoke();
        onAbilityActivatedChanged?.Invoke(selectedIcon.GetAbility());

        if (forceColSwap)
        {
            icon.ForceColorSwap();
        }

        if (playSFX) FMODUWE.PlayOneShot(selectSFX, transform.position, 0.1f);
    }

    private void HandleActivation()
    {
        if (menuOpen) return;

        if (IngameMenu.main.gameObject.activeSelf) return;
        
        if (!MainCameraControl.main.enabled) return;

        if (Player.main.currChair != chair) return;

        if (!GameInput.GetButtonDown(GameInput.Button.RightHand)) return;

        if (!selectedIcon) return;

        if (!selectedIcon.GetAbility().GetCanActivate()) return;

        if (activatedAbilitiesManager.GetActiveAbilityCount() >= 3 && !selectedIcon.GetAbility().GetActive())
        {
            ErrorMessage.AddError("Too many activated abilities");
            return;
        }
        
        if (ProtoStoryLocker.StoryEndingActive)
        {
            notificationManager.PlayVoiceNotification(invalidOperationVoiceline, false);
            return;
        }
        
        bool didActivate = selectedIcon.Activate();
        onAbilityActivatedChanged?.Invoke(selectedIcon.GetAbility());

        if (!didActivate) return;
        
        if (typeToCategory.TryGetValue(selectedIcon.GetAbility().GetTechType(), out var category))
        {
            notificationManager.PlayVO(categoryNotifications[category]);
        }
        else
        {
            notificationManager.PlayVO(genericAbilityNofif);
        }
    }
    
    private RadialIcon GetIconClosestToPointer()
    {
        return distributor.GetIconClosestToAngle(CalculateTetherAngle()).GetComponent<RadialIcon>();
    }

    public RadialIcon GetSelectedIcon()
    {
        return selectedIcon;
    }

    private float CalculateTetherAngle()
    {
        if (timeLastAngleCalculated == Time.time)
        {
            return lastTetherAngle;
        }

        float angle = Mathf.Atan2(tetherPoint.localPosition.y, tetherPoint.localPosition.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        lastTetherAngle = angle;
        timeLastAngleCalculated = Time.time;

        return angle;
    }

    public void SetMenuOpen(bool open)
    {
        menuOpen = open;
        selectionHighlight.gameObject.SetActive(open);
        if (open)
        {
            GoalManager.main.OnCustomGoalEvent("ProtoOpenRadialWheel");
            timeOpened = Time.time;
            FMODUWE.PlayOneShot(openSFX, transform.position, 0.1f);
        }
    }

    public void RegenerateHighlightArc()
    {
        float increment = distributor.GetIncrement();
        selectionHighlight.SetTargetAngle(increment);
    }

    public void ForceSelectedIconUpdate()
    {
        onAbilityActivatedChanged?.Invoke(selectedIcon != null ? selectedIcon.GetAbility() : null);
    }

    public void UpdateIcon(IAbilityIcon abilityIcon)
    {
        onAbilityActivatedChanged?.Invoke(abilityIcon);
    }

    public PilotingChair GetPilotingChair() => chair;
}
