using System;
using Nautilus.Extensions;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.Patches;
using PrototypeSubMod.Utility;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Interceptor;

internal class InterceptorReactorSequenceManager : MonoBehaviour
{
    private static readonly Vector3 VoidTeleportPos = new Vector3(-1590, -562, -288);

    [SaveStateReference]
    private static InterfloorTeleporter Teleporter;
    private static Vector3 MostRecentReturnPos;

    [SerializeField] private MultipurposeAlienTerminal activationTerminal;
    [SerializeField] private InterfloorTeleporter teleporter;
    [SerializeField] private Transform returnPos;
    [SerializeField] private Vector3 islandTeleportPos;
    [SerializeField] private Vector3 voidTeleportPos;

    [Header("Activation Objects")]
    [SerializeField] private GameObject[] inactiveObjects;
    [SerializeField] private GameObject[] activeObjects;
    
    private void Start()
    {
        IngameMenu_Patches.OnQuitToMainMenu += OnQuitToMainMenu;
        
        if (Plugin.GlobalSaveData.reactorSequenceComplete)
        {
            activationTerminal.ForceInteracted();
        }
        else
        {
            activationTerminal.onTerminalInteracted += () =>
            {
                MostRecentReturnPos = returnPos.position;
                StartReactorSequence();
            };
        }

        foreach (var obj in inactiveObjects)
        {
            obj.SetActive(!Plugin.GlobalSaveData.EngineFacilityPointsRepaired);
        }
        foreach (var obj in activeObjects)
        {
            obj.SetActive(Plugin.GlobalSaveData.EngineFacilityPointsRepaired);
        }

        if (!Teleporter)
        {
            var teleporterHolder = new GameObject("IslandTeleporterHolder");
            teleporterHolder.transform.position = new Vector3(0, 50, 0);
            
            Teleporter = teleporterHolder.AddComponent<InterfloorTeleporter>().CopyComponent(teleporter);
        }
    }

    public static void StartReactorSequence()
    {
        UWE.CoroutineHost.StartCoroutine(TeleportToIsland());
    }

    [SaveStateReference(false)]
    public static bool SequenceInProgress;

    public static void EndReactorSequence()
    {
        IngameMenu_Patches.SetDenySaving(false);
        Teleporter.StartTeleportPlayer(MostRecentReturnPos, Camera.main.transform.forward);
        LargeWorldStreamer_Patches.SetOverwriteCamPos(false, Vector3.zero);
        GUIController_Patches.SetDenyHideCycling(false);
        GUIController.SetHidePhase(GUIController.HidePhase.None);
        WeatherCompatManager.SetWeatherEnabled(true);

        Player_Patches.SetOxygenReqOverride(false, 0);
        BiomeGoalTracker_Patches.SetTrackingBlocked(false);
        SequenceInProgress = false;
    }

    public static void OnTeleportToVoid()
    {
        InterceptorIslandManager.Instance.UpdateSeaglideLights(false);
        UWE.CoroutineHost.StartCoroutine(TeleportBackAfterDuration());
        Player_Patches.SetOxygenReqOverride(true, 0);
    }

    private static IEnumerator TeleportBackAfterDuration()
    {
        yield return new WaitUntil(LargeWorldStreamer.main.IsWorldSettled);

        yield return new WaitForSeconds(20f);

        EndReactorSequence();

        yield return new WaitForSeconds(3f);
        
        PDALog.Add("OnInterceptorSequenceFinished");
    }

    private static IEnumerator TeleportToIsland()
    {
        if (SequenceInProgress) yield break;

        SequenceInProgress = true;

        IngameMenu_Patches.SetDenySaving(true);
        BiomeGoalTracker_Patches.SetTrackingBlocked(true);

        InterceptorIslandManager.Instance.OnTeleportToIsland(VoidTeleportPos);
        InterceptorIslandManager.Instance.UpdateSeaglideLights(true);
        WeatherCompatManager.SetWeatherEnabled(false);
        WeatherCompatManager.SetWeatherClear();

        InterfloorTeleporter.PlayTeleportEffect(4f);

        yield return new WaitForSeconds(0.5f);

        LargeWorldStreamer_Patches.SetOverwriteCamPos(true, MostRecentReturnPos);
        Player.main.cinematicModeActive = true;
        Player.main.playerController.inputEnabled = false;
        Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);
        Player.main.GetPDA().Close();
        Player.main.GetPDA().SetIgnorePDAInput(true);
        Player.main.teleportingLoopSound.Play();

        Plugin.GlobalSaveData.reactorSequenceComplete = true;
        Player.main.SetPosition(InterceptorIslandManager.Instance.GetRespawnPoint());

        yield return new WaitForSeconds(4f);

        Player.main.cinematicModeActive = false;
        Player.main.playerController.inputEnabled = true;
        Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
        Player.main.GetPDA().SetIgnorePDAInput(false);
        Player.main.teleportingLoopSound.Stop();
    }

    private void OnQuitToMainMenu()
    {
        LargeWorldStreamer_Patches.SetOverwriteCamPos(false, Vector3.zero);
        IngameMenu_Patches.SetDenySaving(false);
        GUIController_Patches.SetDenyHideCycling(false);
        WeatherCompatManager.SetWeatherEnabled(true);
        Player_Patches.SetOxygenReqOverride(false, 0);
        BiomeGoalTracker_Patches.SetTrackingBlocked(false);
        SequenceInProgress = false;
    }

    private void OnDestroy()
    {
        IngameMenu_Patches.OnQuitToMainMenu -= OnQuitToMainMenu;
    }
}
