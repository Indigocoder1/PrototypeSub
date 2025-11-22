using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using System;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Teleporter;

internal class TeleporterOverride : MonoBehaviour
{
    internal static readonly Color OverrideColor = new(0.5517f, 0.0646f, 1f, 0.4853f);

    public static string FullOverrideTeleporterID { get; private set; }
    public static float OverrideTime { get; private set; }
    public static bool QueuedResetOverrideTime { get; private set; }
    public static bool QueuedTeleportedBackToSub { get; set; }
    private static float TimeWhenPortalUnloaded;
    private static float TimeLeftWhenUnloaded;
    private static bool OverrideRequested;
    private static ProtoTeleporterManager LastOverrideOwner;

    public static event Action<string> OnOverrideRunOut;
    public static event Action<string, float> OnActiveTeleporterUnloaded;
    public static event Action<string> OnUnloadedTeleporterReload;
    private static event Action OnTeleportStart;

    private Vector3 originalTeleportPosition;
    private PrecursorTeleporter teleporter;
    private float originalTeleportAngle;
    private float currentOverrideTime;
    private float overrideTimeLastFrame;
    private string teleporterID;
    private bool overrideActive;

    private Material fxMaterial;
    private Color originalColor;
    private Color targetColor;

    public static void ClearEvents()
    {
        OnOverrideRunOut = null;
        OnActiveTeleporterUnloaded = null;
        OnUnloadedTeleporterReload = null;
        OnTeleportStart = null;
    }
    
    public static void SetOverrideTeleporterID(string id)
    {
        FullOverrideTeleporterID = id;
    }

    public static void SetOverrideTime(float time)
    {
        OverrideTime = time;
    }

    public static void OnTeleportStarted(ProtoTeleporterManager overrideOwner)
    {
        QueuedResetOverrideTime = true;
        OverrideRequested = true;
        OnTeleportStart?.Invoke();
        LastOverrideOwner = overrideOwner;
    }

    public static void OnTeleportToSubFinished()
    {
        QueuedTeleportedBackToSub = false;
        OverrideRequested = false;
        LastOverrideOwner = null;
    }

    private void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        OnTeleportStart += TargetTeleporterCheck;
        Initialize();
    }
    
    private void OnDisable() => OnTeleportStart -= TargetTeleporterCheck;

    public void Initialize()
    {
        // This stuff may look weird but remember that the portal is only loaded in when it's being teleported to, so this is called when it's loaded in

        teleporter = GetComponent<PrecursorTeleporter>();
        if (originalTeleportPosition == Vector3.zero)
        {
            originalTeleportPosition = teleporter.warpToPos;
            originalTeleportAngle = teleporter.warpToAngle;
        }
        
        teleporterID = teleporter.teleporterIdentifier + (IsTeleporterHost(teleporter.teleporterIdentifier) ? "M" : "S");

        if (teleporterID != FullOverrideTeleporterID) return;

        if (overrideActive) return;
        
        UWE.CoroutineHost.StartCoroutine(WaitToPlayFirstStatus());
        
        float timeLeft = TimeWhenPortalUnloaded - Time.time + TimeLeftWhenUnloaded;
        if (QueuedResetOverrideTime)
        {
            currentOverrideTime = OverrideTime;
            QueuedResetOverrideTime = false;
        }
        else if (timeLeft > 0)
        {
            currentOverrideTime = timeLeft;
        }

        overrideActive = true;

        if (TeleporterManager.GetTeleporterActive(teleporterID) && !fxMaterial)
        {
            TryRetrieveFxMaterial();
        }

        OnUnloadedTeleporterReload?.Invoke(teleporterID);
    }

    private bool IsTeleporterHost(string identifier)
    {
        if (!identifier.ToLower().Contains("proto"))
        {
            return GetComponentInParent<PrecursorTeleporterActivationTerminal>() != null;
        }

        return TryGetComponent(out TeleporterHostMarker _);
    }

    private void TargetTeleporterCheck()
    {
        teleporterID = teleporter.teleporterIdentifier + (IsTeleporterHost(teleporter.teleporterIdentifier) ? "M" : "S");

        if (teleporterID != FullOverrideTeleporterID) return;

        if (QueuedResetOverrideTime && !overrideActive)
        {
            Initialize();
        }
    }

    private void Update()
    {
        HandleOverrideCountdown();
        HandleOverrideColor();
    }

    private void HandleOverrideCountdown()
    {
        if (teleporterID != FullOverrideTeleporterID) return;

        if (!OverrideRequested) return;

        if (currentOverrideTime > 0)
        {
            currentOverrideTime -= Time.deltaTime;

            Transform teleportPos = LastOverrideOwner.GetTeleportPosition();
            teleporter.warpToPos = teleportPos.position;
            teleporter.warpToAngle = teleportPos.eulerAngles.y;
        }
        else if (currentOverrideTime <= 0 && overrideActive)
        {
            teleporter.warpToPos = originalTeleportPosition;
            teleporter.warpToAngle = originalTeleportAngle;
            overrideActive = false;
            OnOverrideRunOut?.Invoke(teleporterID);
        }

        if (overrideTimeLastFrame > 30f && currentOverrideTime <= 30f)
        {
            LastOverrideOwner.PlayOverrideMarker2();
        }

        overrideTimeLastFrame = currentOverrideTime;
    }

    private void HandleOverrideColor()
    {
        if (fxMaterial == null)
        {
            TryRetrieveFxMaterial();
            return;
        }

        targetColor = overrideActive ? OverrideColor : originalColor;

        Color color = Color.Lerp(fxMaterial.GetColor("_ColorOuter"), targetColor, Time.deltaTime);

        fxMaterial.SetColor("_ColorOuter", color);
    }

    private void TryRetrieveFxMaterial()
    {
        var rend = teleporter.portalFxSpawnPoint.GetComponentInChildren<MeshRenderer>(true);

        if (!rend) return;

        fxMaterial = rend.material;
        originalColor = fxMaterial.GetColor("_ColorOuter");
    }

    public void BeginTeleportPlayer(GameObject _)
    {
        if (!overrideActive) return;
        
        QueuedTeleportedBackToSub = true;
        overrideActive = false;
        Player.main.SetPrecursorOutOfWater(false);

        teleporter.warpToPos = originalTeleportPosition;
        teleporter.warpToAngle = originalTeleportAngle;
        
        var teleportManager = Camera.main.GetComponent<ProtoScreenTeleporterFXManager>();
        teleportManager.SetColors(ProtoTeleporterManager.TeleportScreenColInner, ProtoTeleporterManager.TeleportScreenColMiddle, ProtoTeleporterManager.TeleportScreenColOuter);
    }

    // Called in PrecursorTeleporterActivationTerminal via BroadcastMessage
    public void ToggleDoor(bool _)
    {
        if (!fxMaterial) TryRetrieveFxMaterial();
    }

    private void OnDestroy()
    {
        if (overrideActive)
        {
            TimeWhenPortalUnloaded = Time.time;
            TimeLeftWhenUnloaded = currentOverrideTime;
            OnActiveTeleporterUnloaded?.Invoke(teleporterID, currentOverrideTime);
        }
    }

    private IEnumerator WaitToPlayFirstStatus()
    {
        yield return new WaitUntil(LargeWorldStreamer.main.IsWorldSettled);

        yield return new WaitForSeconds(2f);

        if (LastOverrideOwner != null)
        {
            LastOverrideOwner.PlayOverrideMarker1();
        }
    }
}
