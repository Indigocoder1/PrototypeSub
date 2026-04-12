using System;
using Nautilus.Extensions;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.Patches;
using PrototypeSubMod.Utility;
using System.Collections;
using PrototypeSubMod.IonGenerator;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Interceptor;

internal class InterceptorReactorSequenceManager : MonoBehaviour
{
    private static readonly Vector3 VoidTeleportPos = new (-1590, -562, -288);
    private static event Action OnSequenceCompleteEvent;
    
    [SaveStateReference]
    private static InterfloorTeleporter _teleporter;
    private static Vector3 _mostRecentReturnPos;
    
    [SerializeField] private InterfloorTeleporter teleporter;
    [SerializeField] private RadiatePlayerInRange radiatePlayerInRange;
    [SerializeField] private EmpSpawner empSpawner;
    [SerializeField] private FMOD_CustomEmitter empSfx;
    [SerializeField] private Animator warpCoreAnimator;
    [SerializeField] private AnimationCurve animationSpeedOverDistance;
    [SerializeField] private Color closeRadiationColor;
    [SerializeField] private float pdaMessageDistance;
    [SerializeField] private float teleportationDistance;

    [Header("Activation Objects")]
    [SerializeField] private GameObject[] inactiveObjects;
    [SerializeField] private GameObject[] activeObjects;

    private RadiationsScreenFX radiationsScreenFX;
    private uGUI_RadiationWarning radiationWarning;
    private Color originalRadiationColor;
    private bool wasOutOfRange;
    
    private void Start()
    {
        IngameMenu_Patches.OnQuitToMainMenu += OnQuitToMainMenu;
        OnSequenceCompleteEvent += OnSequenceComplete;
        radiationsScreenFX = Camera.main.GetComponent<RadiationsScreenFX>();
        originalRadiationColor = radiationsScreenFX.color;

        radiationWarning = uGUI.main.transform.Find("ScreenCanvas/HUD/Content/RadiationWarning")
            .GetComponent<uGUI_RadiationWarning>();

        foreach (var obj in inactiveObjects)
        {
            obj.SetActive(!Plugin.GlobalSaveData.EngineFacilityPointsRepaired);
        }
        foreach (var obj in activeObjects)
        {
            obj.SetActive(Plugin.GlobalSaveData.EngineFacilityPointsRepaired);
        }

        if (!_teleporter)
        {
            var teleporterHolder = new GameObject("IslandTeleporterHolder");
            teleporterHolder.transform.position = new Vector3(0, 50, 0);
            
            _teleporter = teleporterHolder.AddComponent<InterfloorTeleporter>().CopyComponent(teleporter);
        }

        radiatePlayerInRange.enabled = Plugin.GlobalSaveData.EngineFacilityPointsRepaired;
    }

    private void Update()
    {
        if (!Plugin.GlobalSaveData.EngineFacilityPointsRepaired || Plugin.GlobalSaveData.reactorSequenceComplete) return;

        var distance = Vector3.Distance(transform.position, Player.main.transform.position);
        warpCoreAnimator.speed = animationSpeedOverDistance.Evaluate(distance);

        if (distance < pdaMessageDistance)
        {
            PDALog.Add("PDA_OnApproachWarpCore");
        }

        bool outOfRange = distance > pdaMessageDistance;

        if (outOfRange != wasOutOfRange)
        {
            radiationsScreenFX.color = outOfRange ? originalRadiationColor : closeRadiationColor;
            radiationWarning.text.text = Language.main.Get(outOfRange ? "RadiationDetected": "DarkMatterDetected");
        }
        
        if (distance < teleportationDistance && !SequenceInProgress)
        {
            _mostRecentReturnPos = Player.main.transform.position;
            StartReactorSequence();
            Player.main.TryEject();
        }

        wasOutOfRange = distance > pdaMessageDistance;
    }

    private void StartReactorSequence()
    {
        UWE.CoroutineHost.StartCoroutine(TeleportToIsland());
    }

    [SaveStateReference(false)]
    public static bool SequenceInProgress;

    public static void EndReactorSequence()
    {
        IngameMenu_Patches.SetDenySaving(false);
        _teleporter.StartTeleportPlayer(_mostRecentReturnPos, Camera.main.transform.forward);
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

    private IEnumerator TeleportToIsland()
    {
        if (SequenceInProgress) yield break;

        SequenceInProgress = true;

        IngameMenu_Patches.SetDenySaving(true);
        BiomeGoalTracker_Patches.SetTrackingBlocked(true);

        InterceptorIslandManager.Instance.OnTeleportToIsland(VoidTeleportPos);
        InterceptorIslandManager.Instance.UpdateSeaglideLights(true);
        WeatherCompatManager.SetWeatherEnabled(false);
        WeatherCompatManager.SetWeatherClear();

        empSfx.Play();
        empSpawner.FireEMP(0);
        var distToPlayer = Vector3.Distance(empSpawner.transform.position, Player.main.transform.position);
        // Wait until the EMP hits the player
        yield return new WaitForSeconds(distToPlayer / empSpawner.GetFinalRadius() * empSpawner.GetLifetime() * 0.5f);

        InterfloorTeleporter.PlayTeleportEffect(3f);

        yield return new WaitForSeconds(0.5f);

        LargeWorldStreamer_Patches.SetOverwriteCamPos(true, _mostRecentReturnPos);
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

    private void OnSequenceComplete()
    {
        radiationsScreenFX.color = originalRadiationColor;
        radiationWarning.text.text = Language.main.Get("RadiationDetected");
        radiatePlayerInRange.enabled = false;
    }

    private void OnDestroy()
    {
        IngameMenu_Patches.OnQuitToMainMenu -= OnQuitToMainMenu;
        OnSequenceCompleteEvent -= OnSequenceComplete;
        radiationsScreenFX.color = originalRadiationColor;
    }
}
