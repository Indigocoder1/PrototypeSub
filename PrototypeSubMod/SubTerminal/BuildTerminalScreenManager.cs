using System;
using Story;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.SubTerminal;

internal class BuildTerminalScreenManager : MonoBehaviour
{
    [SerializeField] private uGUI_FirstInteractScreen firstInteractScreen;
    [SerializeField] private uGUI_ProtoBuildScreen buildScreen;
    [SerializeField] private TerminalScreen rebuildScreen;
    [SerializeField] private TerminalScreen animatorScreen;
    [SerializeField] private NewUpgradesScreen newUpgradesScreen;
    [SerializeField] private GameObject upgradeScreen;
    [SerializeField] private GameObject recentralizeScreen;
    [SerializeField] private MoonpoolOccupiedHandler occupiedHandler;

    private bool hadSubLastFrame;
    private bool isBuilding;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        animatorScreen.gameObject.SetActive(false);
        newUpgradesScreen.gameObject.SetActive(false);
        rebuildScreen.gameObject.SetActive(false);
        buildScreen.gameObject.SetActive(false);

        EnableRelevantScreens();

        bool hasInteracted = StoryGoalManager.main.completedGoals.Contains("PlayerFirstPPTInteraction");
        if (!hasInteracted)
        {
            firstInteractScreen.OnStageStarted();
            buildScreen.gameObject.SetActive(false);
            upgradeScreen.gameObject.SetActive(false);
            recentralizeScreen.gameObject.SetActive(false);
        }
        else
        {
            firstInteractScreen.gameObject.SetActive(false);
        }

        firstInteractScreen.UpdateLightingController();
        occupiedHandler.onHasSubChanged += EnableRelevantScreens;
    }

    public void OnConstructionStarted()
    {
        isBuilding = true;
        rebuildScreen.gameObject.SetActive(false);
    }

    // Called by VFX Constructing in Update via SendMessage()
    public void OnConstructionDone()
    {
        isBuilding = false;
    }

    private void Update()
    {
        if (occupiedHandler.MoonpoolHasSub != hadSubLastFrame)
        {
            OnOccupiedChanged();
        }

        hadSubLastFrame = occupiedHandler.MoonpoolHasSub;
    }

    private void OnOccupiedChanged()
    {
        if (isBuilding) return;

        EnableMenusWhenSubInWorld();
    }

    public void BeginWaitForBuildStage()
    {
        firstInteractScreen.OnStageFinished();
        buildScreen.OnStageStarted();
        recentralizeScreen.SetActive(false);
    }

    public void BeginBuildStage()
    {
        buildScreen.OnStageFinished();
        animatorScreen.OnStageStarted();
        rebuildScreen.gameObject.SetActive(false);
        recentralizeScreen.SetActive(false);
    }

    public void EndBuildStage()
    {
        animatorScreen.OnStageFinished();
        recentralizeScreen.SetActive(false);
        newUpgradesScreen.gameObject.SetActive(false);
        upgradeScreen.gameObject.SetActive(true);
        occupiedHandler.CheckForSub();

        EnableMenusWhenSubInWorld();
    }

    public void EnableRelevantScreens()
    {
        newUpgradesScreen.gameObject.SetActive(false);
        if (Plugin.GlobalSaveData.prototypeDestroyed && StoryGoalManager.main.IsGoalComplete("PrototypeCrafted"))
        {
            rebuildScreen.gameObject.SetActive(true);

            buildScreen.gameObject.SetActive(false);
            upgradeScreen.gameObject.SetActive(false);
            recentralizeScreen.gameObject.SetActive(false);
        }
        else if (Plugin.GlobalSaveData.prototypePresent && StoryGoalManager.main.IsGoalComplete("PrototypeCrafted"))
        {
            occupiedHandler.CheckForSub();
            EnableMenusWhenSubInWorld();
        }
        else
        {
            buildScreen.gameObject.SetActive(true);
            rebuildScreen.gameObject.SetActive(false);
            upgradeScreen.gameObject.SetActive(false);
            recentralizeScreen.gameObject.SetActive(false);
        }
    }

    public void EnableMenusWhenSubInWorld()
    {
        buildScreen.gameObject.SetActive(false);
        rebuildScreen.gameObject.SetActive(false);
        
        if (newUpgradesScreen.HasQueuedUnlocks() && occupiedHandler.MoonpoolHasSub)
        {
            newUpgradesScreen.gameObject.SetActive(true);
            upgradeScreen.gameObject.SetActive(false);
            recentralizeScreen.gameObject.SetActive(false);
        }
        else
        {
            newUpgradesScreen.gameObject.SetActive(false);
            upgradeScreen.gameObject.SetActive(occupiedHandler.MoonpoolHasSub);
            recentralizeScreen.gameObject.SetActive(!occupiedHandler.MoonpoolHasSub);
        }
    }

    private void OnEnable()
    {
        EnableRelevantScreens();
    }
}
