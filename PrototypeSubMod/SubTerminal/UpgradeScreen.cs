using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.SubTerminal;

[RequireComponent(typeof(CanvasGroup))]
internal class UpgradeScreen : MonoBehaviour
{
    [SerializeField] private GameObject unlockBlockerImage;
    [SerializeField] private int maxAllowedUpgrades = 1;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float startingAlpha;

    private CanvasGroup canvasGroup;
    private float targetAlpha;
    private TechType[] availableUpgrades;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        targetAlpha = startingAlpha;
        canvasGroup.alpha = startingAlpha;
        canvasGroup.blocksRaycasts = startingAlpha > 0;

        var icons = GetComponentsInChildren<uGUI_ProtoUpgradeIcon>(true);
        availableUpgrades = new TechType[icons.Length];
        for (int i = 0; i < icons.Length; i++)
        {
            availableUpgrades[i] = icons[i].GetUpgradeTechType();
        }

        CheckIfUnlocked(TechType.None, false);
    }

    // The verbose bit is needed for the KnownTech event
    private void CheckIfUnlocked(TechType techType, bool verbose)
    {
        if (unlockBlockerImage == null) return;

        bool unlocked = false;
        foreach (var type in availableUpgrades)
        {
            unlocked |= KnownTech.Contains(type);
            if (unlocked) break;
        }

        unlockBlockerImage.SetActive(!unlocked);
    }

    private void Update()
    {
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * transitionSpeed);
    }

    public void SetTargetAlpha(float alpha)
    {
        targetAlpha = alpha;
    }

    public void SetInteractable(bool interactable)
    {
        canvasGroup.blocksRaycasts = interactable;
    }

    private void OnEnable()
    {
        KnownTech.onAdd += CheckIfUnlocked;
        CheckIfUnlocked(TechType.None, false);
    }
    
    private void OnDisable() => KnownTech.onAdd -= CheckIfUnlocked;
}
