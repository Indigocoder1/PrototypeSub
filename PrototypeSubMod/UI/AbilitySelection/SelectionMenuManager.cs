using System.Collections;
using PrototypeSubMod.Patches;
using PrototypeSubMod.Upgrades;
using PrototypeSubMod.Utility;
using SubLibrary.UI;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace PrototypeSubMod.UI.AbilitySelection;

internal class SelectionMenuManager : MonoBehaviour, IUIElement
{
    [SerializeField] private List<GameObject> abilities;
    [SerializeField] private int defaultAbilityIndex;
    [SerializeField] private IconDistributor distributor;
    [SerializeField] private TetherManager tetherManager;
    [SerializeField] private ProtoUpgradeManager upgradeManager;
    [SerializeField] private Animator menuManager;
    [SerializeField] private Transform cameraFocus;
    [SerializeField] private float maxAlignmentSpeed;
    
    [Header("Engine hint fade")]
    [SerializeField] private CanvasGroup engineHint;
    [SerializeField] private float fadeDuration;

    [SerializeField, HideInInspector] public List<IAbilityIcon> abilityIcons = new();
    private List<IAbilityIcon> iconsToShow = new();
    private bool menuEnabled;
    private PilotingChair chair;
    private Coroutine fadeRoutine;

    private void OnValidate()
    {
        abilityIcons.Clear();

        for (int i = abilities.Count - 1; i >= 0; i--)
        {
            var ability = abilities[i].GetComponent<IAbilityIcon>();
            if (ability == null)
            {
                abilities.RemoveAt(i);
            }
            else
            {
                abilityIcons.Add(ability);
            }
        }
    }

    private void Start()
    {
        UWE.CoroutineHost.StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        AssignIcons();
        RefreshIcons();
        upgradeManager.onInstalledUpgradesChanged += RefreshIcons;
        tetherManager.onNewAbilitySelected += () => SetMenuEnabled(false);
        
        yield return new WaitForEndOfFrame();
        
        chair = tetherManager.GetPilotingChair();
        SelectDefaultIcon();
    }

    public void SelectDefaultIcon()
    {
        var defaultIcon = distributor.GetIconAtIndex(defaultAbilityIndex).GetComponent<RadialIcon>();
        tetherManager.SelectIcon(defaultIcon, true, true, false);
        tetherManager.onAbilityActivatedChanged?.Invoke(defaultIcon.GetAbility());
    }

    private void Update()
    {
        if (menuEnabled && GameInput.GetButtonDown(GameInput.Button.RightHand))
        {
            SetMenuEnabled(false);
        }

        if (Player.main.currChair == chair || !menuEnabled) return;

        SetMenuEnabled(false);
    }

    private void RetrieveIconsToShow()
    {
        iconsToShow.Clear();
        foreach (var ability in abilityIcons)
        {
            if (!ability.GetShouldShow()) continue;
            iconsToShow.Add(ability);
        }
    }

    public void RefreshIcons()
    {
        RetrieveIconsToShow();
        var icon = tetherManager.GetSelectedIcon();
        int index = 0;
        IAbilityIcon selectedAbility = null;
        if (icon)
        {
            selectedAbility = icon.GetAbility();
            index = tetherManager.GetSelectedIcon().transform.GetSiblingIndex();
        }
        
        distributor.RegenerateIcons(iconsToShow);

        StartCoroutine(SelectDelayed(selectedAbility, index));
        
        tetherManager.RegenerateHighlightArc();
    }

    private IEnumerator SelectDelayed(IAbilityIcon selectedAbility, int index)
    {
        yield return new WaitForEndOfFrame();
        
        if (selectedAbility != null && !selectedAbility.GetShouldShow())
        {
            selectedAbility.OnSelectedChanged(false);
            tetherManager.SelectIcon(distributor.GetIconAtIndex(defaultAbilityIndex).GetComponent<RadialIcon>(), true, playSFX: false);
        }
        else if (selectedAbility != null)
        {
            tetherManager.SelectIcon(distributor.GetIconAtIndex(index).GetComponent<RadialIcon>(), playSFX: false, closeIfSameAbility: false);
        }
    }

    private void AssignIcons()
    {
        foreach (var item in abilities)
        {
            var ability = item.GetComponent<IAbilityIcon>();
            abilityIcons.Add(ability);
        }
    }

    public void UpdateUI()
    {
        if (!menuEnabled) return;

        MainCameraControl_Patches.SetOverwriteDelta(ProtoCameraUtils.CalculateTargetAngleDelta(cameraFocus, maxAlignmentSpeed), true);
    }

    public void SetMenuEnabled(bool enabled)
    {
        tetherManager.SetMenuOpen(enabled);
        menuEnabled = enabled;
        menuManager.SetBool("MenuOpen", enabled);

        if (!enabled)
        {
            MainCameraControl_Patches.SetOverwriteDelta(Vector2.zero, false);
            StartEngineHintFade(1f);
        }
        else
        {
            StartEngineHintFade(0f);
        }
    }

    public void OpenMenu()
    {
        if (menuEnabled) return;

        SetMenuEnabled(true);
    }
    
    private void StartEngineHintFade(float targetAlpha)
    {
        if (fadeRoutine != null)
            UWE.CoroutineHost.StopCoroutine(fadeRoutine);

        fadeRoutine = UWE.CoroutineHost.StartCoroutine(FadeEngineHint(targetAlpha));
    }
    
    private IEnumerator FadeEngineHint(float targetAlpha)
    {
        if (!engineHint) yield break;
        float startAlpha = engineHint.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            engineHint.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        engineHint.alpha = targetAlpha;
    }

    private void OnDestroy()
    {
        MainCameraControl_Patches.SetOverwriteDelta(Vector2.zero, false);
    }

    public void OnSubDestroyed()
    {
        MainCameraControl_Patches.SetOverwriteDelta(Vector2.zero, false);
    }

    public List<IAbilityIcon> GetAbilityIcons()
    {
        return iconsToShow;
    }
}
