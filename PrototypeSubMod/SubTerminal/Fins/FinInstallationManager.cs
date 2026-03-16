using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.SubTerminal.Relays;
using Story;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.SubTerminal.Fins;

public class FinInstallationManager : MonoBehaviour
{
    [SerializeField] private Transform buildPosition;
    [SerializeField] private MoonpoolOccupiedHandler occupiedHandler;
    [SerializeField] private FinInstallationButton[] installationButtons;
    [SerializeField] private Image homeScreen;
    [SerializeField] private Sprite[] homeScreenSprites;
    
    private ProtoFinsManager finsManager;

    private bool hadSubLastFrame;
    
    private void Start()
    {
        UpdateIcons();
    }

    private void Update()
    {
        if (occupiedHandler.MoonpoolHasSub != hadSubLastFrame)
        {
            OnHasSubChanged();
        }

        hadSubLastFrame = occupiedHandler.MoonpoolHasSub;
    }

    private void OnHasSubChanged()
    {
        if (!occupiedHandler.MoonpoolHasSub) return;
        
        finsManager = occupiedHandler.SubInMoonpool.GetComponentInChildren<ProtoFinsManager>();
        UpdateIcons();
    }

    private void UpdateIcons()
    {
        if (finsManager == null) return;
        
        for (int i = 0; i < installationButtons.Length; i++)
        {
            var button = installationButtons[i];
            button.SetCanBuild(i == finsManager.GetInstalledFinCount());
            button.SetConstructed(i < finsManager.GetInstalledFinCount());
        }

        UpdateHomeScreenIcon();
    }

    private void UpdateHomeScreenIcon()
    {
        homeScreen.sprite = homeScreenSprites[finsManager.GetInstalledFinCount()];
    }

    public void InstallNewFin()
    {
        if (!finsManager)
        {
            Plugin.Logger.LogError("Trying to install new fin without reference to fins manager");
            return;
        }

        var pos = occupiedHandler.SubInMoonpool.transform.position;
        pos.y = buildPosition.position.y;
        occupiedHandler.SubInMoonpool.transform.position = pos;

        installationButtons[Mathf.Min(finsManager.GetInstalledFinCount(), installationButtons.Length - 1)].LockTechType();
        finsManager.SetInstalledFinCount(finsManager.GetInstalledFinCount() + 1);
        if (finsManager.GetInstalledFinCount() < installationButtons.Length)
        {
            installationButtons[finsManager.GetInstalledFinCount()].UnlockTechType();
        }
        UpdateIcons();

        if (!StoryGoalManager.main.IsGoalComplete("FinsFirstInstalled") && finsManager.GetInstalledFinCount() == 1)
        {
            StoryGoalManager.main.OnGoalComplete("FinsFirstInstalled");
        }

    }
}