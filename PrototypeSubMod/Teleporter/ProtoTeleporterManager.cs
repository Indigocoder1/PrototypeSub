using System;
using System.Collections;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.Upgrades;
using PrototypeSubMod.Facilities.Interceptor;
using Story;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PrototypeSubMod.Teleporter;

internal class ProtoTeleporterManager : ProtoUpgrade
{
    private const float OverrideDuration = 120f;
    
    internal static readonly Color TeleportScreenColInner = new Color(0.5638f, 0.4349f, 0.6674f, 0.4970f);
    internal static readonly Color TeleportScreenColMiddle = new Color(0.15f, 0.1905f, 1.0000f, 0.3000f);
    internal static readonly Color TeleportScreenColOuter = new Color(0.4412f, 0.4285f, 0.7118f, 0.4790f);
    
    [Header("Teleporting")]
    [SerializeField] private PrecursorTeleporter teleporter;
    [SerializeField] private TeleporterModeManager modeManager;
    [SerializeField] private ProtoTeleporterIDManager idManager;
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private Transform teleportPosition;
    [SerializeField] private FMOD_CustomLoopingEmitter activeLoopSound;
    [SerializeField] private VoiceNotification overrideStatus1;
    [SerializeField] private VoiceNotification overrideStatus2;
    [SerializeField] private string teleporterID;
    
    private float timeOverrideTeleporterUnloaded;
    private bool overrideTeleporterUnloaded;
    private string unloadedTeleporterID;

    private void OnValidate()
    {
        if (!teleporter) TryGetComponent(out teleporter);
    }

    private void Start()
    {
        TeleporterOverride.ClearEvents();
        PrecursorTeleporter.TeleportEventEnd += OnTeleportEnd;
        TeleporterManager.main.activeTeleporters.Remove("prototypetp");
        TeleporterOverride.OnActiveTeleporterUnloaded += (id, time) =>
        {
            timeOverrideTeleporterUnloaded = time;
            overrideTeleporterUnloaded = id == GetTeleporterIDNoIndicator();
            unloadedTeleporterID = id;
        };

        TeleporterOverride.OnUnloadedTeleporterReload += (id) =>
        {
            if (id != unloadedTeleporterID) return;
            
            overrideTeleporterUnloaded = false;
        };

        TeleporterOverride.OnOverrideRunOut += (id) =>
        {
            idManager.UnselectAll();
            modeManager.SetInterfloorMode();
        };
        
        activeLoopSound.Stop();
    }

    private void Update()
    {
        if (overrideTeleporterUnloaded && (timeOverrideTeleporterUnloaded + OverrideDuration) < Time.time)
        {
            modeManager.SetInterfloorMode();
            overrideTeleporterUnloaded = false;
        }
    }

    // Called by PrecursorTeleporterCollider.OnTriggerEnter via SendMessageUpwards
    public void BeginTeleportPlayer(GameObject player)
    {
        Plugin.Logger.LogInfo($"Starting teleport player to {teleporterID}");
        
        TeleporterPositionHandler.TeleportData positionData = default;
        if (!TeleporterPositionHandler.TeleporterPositions.TryGetValue(teleporterID, out positionData))
        {
            throw new System.Exception($"Tried to teleport to unknown ID: \"{teleporterID}\". Position unknown");
        }

        teleporter.warpToPos = positionData.teleportPosition;
        teleporter.warpToAngle = positionData.teleportAngle;

        TeleporterOverride.SetOverrideTeleporterID(teleporterID);
        TeleporterOverride.SetOverrideTime(OverrideDuration);
        TeleporterOverride.OnTeleportStarted(this);

        Camera.main.GetComponent<ProtoScreenTeleporterFXManager>().SetColors(TeleportScreenColInner, TeleportScreenColMiddle, TeleportScreenColOuter);
        
        if (teleporterID == "protoislandtpS")
        {
            InterceptorIslandManager.Instance.OnTeleportToIsland(Vector3.zero);
            UWE.CoroutineHost.StartCoroutine(InitTeleporterOverrideDelayed());
        }
    }

    private IEnumerator InitTeleporterOverrideDelayed()
    {
        yield return new WaitUntil(() => LargeWorldStreamer.main.IsWorldSettled());
        
        InterceptorIslandManager.Instance.GetComponentsInChildren<TeleporterOverride>(true).Initialize();
    }

    public void SetTeleporterID(string id)
    {
        Plugin.Logger.LogInfo($"Setting teleporter ID to {id}");
        teleporterID = id;
    }

    private void OnTeleportEnd()
    {
        if (!TeleporterOverride.QueuedTeleportedBackToSub) return;

        TeleporterOverride.OnTeleportToSubFinished();
        modeManager.SetInterfloorMode();

        Player.main.SetCurrentSub(subRoot, true);
        idManager.UnselectAll();
    }

    public Transform GetTeleportPosition() => teleportPosition;
    public string GetTeleporterID() => teleporterID;
    public AssetReferenceGameObject GetEndCinematicController() => teleporter.cinematicEndControllerPrefabReference;

    /// <summary>
    /// Returns the teleporter ID without the M/S indicator
    /// </summary>
    /// <returns></returns>
    public string GetTeleporterIDNoIndicator()
    {
        return teleporterID.Replace("M", string.Empty).Replace("S", string.Empty);
    }

    public void PlayOverrideMarker1()
    {
        overrideStatus1.Play();
    }

    public void PlayOverrideMarker2()
    {
        overrideStatus2.Play();
    }

    private void OnDestroy()
    {
        PrecursorTeleporter.TeleportEventEnd -= OnTeleportEnd;
    }

    public override bool GetUpgradeEnabled() => upgradeInstalled;
    
    public override bool OnActivated() => false;
    public override void OnSelectedChanged(bool selected) { }
    public override bool GetShouldShow() => false;
}

public struct ColorOverrideData
{
    public bool overrideActive;
    public Color innerColor;
    public Color middleColor;
    public Color outerColor;

    public ColorOverrideData(bool overrideActive, Color innerColor, Color middleColor, Color outerColor)
    {
        this.overrideActive = overrideActive;
        this.innerColor = innerColor;
        this.middleColor = middleColor;
        this.outerColor = outerColor;
    }
}