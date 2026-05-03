using System;
using System.Collections;
using System.Linq;
using Nautilus.Json;
using Nautilus.Utility;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using UnityEngine;

namespace PrototypeSubMod.PhaseGates;

internal class ProtoPhaseGateManager : MonoBehaviour, IProtoTreeEventListener
{
    public static event Action OnPhaseGateDeactivated;
    
    [SerializeField] private PrecursorTeleporter teleporter;
    [SerializeField] private PrefabIdentifier prefabIdentifier;
    [SerializeField] private LightingController lightingController;
    [SerializeField] private Vector3 localTeleportPos;
    
    [Header("SFX")]
    [SerializeField] private FMOD_CustomLoopingEmitter ambienceSfx;
    [SerializeField] private FMOD_CustomEmitter constructSfx;
    [SerializeField] private FMOD_CustomEmitter impulseConnected;
    [SerializeField] private FMOD_CustomEmitter impulseNotConnected;
    
    private int gateIndex;
    private bool playerWasPiloting;
    private PhaseGateLocation connectedGateLocation;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        
        if (TeleporterManager.main.activeTeleporters.Contains(teleporter.teleporterIdentifier))
        {
            lightingController.SnapToState(1);
            ambienceSfx.Play();
        }
    }

    private void UpdateConnectedGate()
    {
        if (Plugin.GlobalSaveData.phaseGateLocations.Count % 2 == 1) return;

        var index = (gateIndex + 1) % 2;
        connectedGateLocation = Plugin.GlobalSaveData.phaseGateLocations.Values.ElementAt(index);

        var matrix = Matrix4x4.TRS(connectedGateLocation.Position,
            Quaternion.LookRotation(connectedGateLocation.TeleporterForward), Vector3.one);
        teleporter.warpToPos = matrix.MultiplyPoint3x4(localTeleportPos);
        var forward = connectedGateLocation.TeleporterForward;
        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        teleporter.warpToAngle = angle;
    }

    public void SetGateIndex(int index)
    {
        gateIndex = index;
        Plugin.GlobalSaveData.phaseGateIndices[prefabIdentifier.Id] = gateIndex;
        UpdateConnectedGate();
    }

    public void OnConstructionStarted()
    {
        StartCoroutine(EnableLightsDelayed());
        constructSfx.Play();
    }

    public void OnConstructionFinished()
    {
        constructSfx.Stop();
        StartCoroutine(PlayImpulseDelayed());
    }

    private IEnumerator PlayImpulseDelayed()
    {
        yield return new WaitForEndOfFrame();

        if (teleporter.isOpen)
        {
            impulseConnected.Play();
        }
        else
        {
            impulseNotConnected.Play();
        }
    }

    private IEnumerator EnableLightsDelayed()
    {
        yield return new WaitForEndOfFrame();
        
        lightingController.LerpToState(1);
    }

    public void ActivateGate()
    {
        teleporter.ToggleDoor(true);
        ambienceSfx.Play();
    }

    public IEnumerator DeconstructGate(float timeToDeconstruct)
    {
        constructSfx.Play();
        
        var vfxConstructing = GetComponent<VFXConstructing>();
        vfxConstructing.ghostOverlay = vfxConstructing.gameObject.EnsureComponent<VFXOverlayMaterial>();
        vfxConstructing.ghostMaterial = new Material(MaterialUtils.GhostMaterial);
        vfxConstructing.ghostMaterial.color = GetComponent<GhostMaterialSetter>().GetGhostColor();
        vfxConstructing.ghostOverlay.ApplyOverlay(vfxConstructing.ghostMaterial, "VFXDeconstructing", false);
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (var material in renderer.materials)
            {
                material.EnableKeyword("FX_BUILDING");
                material.SetTexture(ShaderPropertyID._EmissiveTex, vfxConstructing.alphaDetailTexture);
                material.SetColor(ShaderPropertyID._BorderColor, vfxConstructing.wireColor);
                material.SetFloat(ShaderPropertyID._Built, 0f);
                material.SetFloat(ShaderPropertyID._Cutoff, 0.42f);
                material.SetVector(ShaderPropertyID._BuildParams, new Vector4(0.035f, 0.07f, 0.08f, -0.12f));
                material.SetFloat(ShaderPropertyID._NoiseStr, 1.9f);
                material.SetFloat(ShaderPropertyID._NoiseThickness, 0.52f);
                material.SetFloat(ShaderPropertyID._BuildLinear, 0f);
                material.SetFloat(ShaderPropertyID._MyCullVariable, 0f);
            }
        }

        Shader.SetGlobalFloat(ShaderPropertyID._SubConstructProgress, 1);

        yield return new WaitForSeconds(0.1f);

        float timer = timeToDeconstruct;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            Shader.SetGlobalFloat(ShaderPropertyID._SubConstructProgress, timer / timeToDeconstruct);
            yield return null;
        }

        constructSfx.Stop();
    }
    
    public void DeactivateGate()
    {
        TeleporterManager.main.activeTeleporters.Remove(teleporter.teleporterIdentifier);
        OnPhaseGateDeactivated?.Invoke();
        lightingController.LerpToState(0);
        ambienceSfx.Stop();
    }

    // Called via SendMessageUpwards in PrecursorTeleporterCollider
    public void BeginTeleportSubRoot(SubRoot subRoot)
    {
        if (subRoot == null)
        {
            return;
        }
        if (PrecursorTeleporter.activeTeleporter != null)
        {
            return;
        }
        if (!TeleporterManager.GetTeleporterActive(teleporter.teleporterIdentifier))
        {
            return;
        }

        StartCoroutine(TeleportSubRoot(subRoot.gameObject));
    }

    private IEnumerator TeleportSubRoot(GameObject subRoot)
    {
        PrecursorTeleporter.activeTeleporter = teleporter;
        var subRigidbody = subRoot.GetComponent<Rigidbody>();
        var pilotingChair = subRoot.GetComponentInChildren<PilotingChair>();

        playerWasPiloting = Player.main.currChair == pilotingChair;
        if (playerWasPiloting)
        {
            var player = Player.main;
            player.AddUsedTool(TechType.PrecursorTeleporter);

            player.cinematicModeActive = true;
            player.playerController.inputEnabled = false;
            Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);
            player.GetPDA().Close();
            player.GetPDA().SetIgnorePDAInput(true);
            player.teleportingLoopSound.Play();
            Player.mainCollider.enabled = false;
        
            player.onTeleportationComplete += () => OnTeleportationComplete(subRigidbody, pilotingChair);
            Camera.main.GetComponent<TeleportScreenFXController>().StartTeleport();
        }

        if (playerWasPiloting)
        {
            subRigidbody.isKinematic = true;
        }
        
        subRigidbody.velocity = Vector3.zero;

        yield return new WaitForSeconds(1f);
        
        subRoot.transform.position = teleporter.warpToPos;
        subRoot.transform.rotation = Quaternion.Euler(0, teleporter.warpToAngle, 0);

        if (playerWasPiloting)
        {
            Player.main.WaitForTeleportation();
        }
    }
    
    private void OnTeleportationComplete(Rigidbody subRigidbody, PilotingChair pilotingChair)
    {
        subRigidbody.isKinematic = false;
        Player.mainCollider.enabled = true;

        if (PrecursorTeleporter.activeTeleporter == teleporter)
        {
            PrecursorTeleporter.activeTeleporter = null;
        }

        StartCoroutine(ReEnterPilotingModeDelayed(pilotingChair));
        playerWasPiloting = false;
    }
    
    private IEnumerator ReEnterPilotingModeDelayed(PilotingChair pilotingChair)
    {
        yield return new WaitForEndOfFrame();
        
        Player.main.EnterPilotingMode(pilotingChair);
    }
    
    private void OnGateDeactivated()
    {
        if (TeleporterManager.main.activeTeleporters.Contains(teleporter.teleporterIdentifier)) return;
        
        teleporter.ToggleDoor(false);
        teleporter.activeLoopSound.Stop();
        ambienceSfx.Stop();
    }
    
    private void OnEnable()
    {
        Plugin.GlobalSaveData.OnStartedSaving += OnStartedSaving;
        PhaseGateSubAbility.onPhaseGateCreated += UpdateConnectedGate;
        OnPhaseGateDeactivated += OnGateDeactivated;
    }

    private void OnDisable()
    {
        Plugin.GlobalSaveData.OnStartedSaving -= OnStartedSaving;
        PhaseGateSubAbility.onPhaseGateCreated -= UpdateConnectedGate;
        OnPhaseGateDeactivated -= OnGateDeactivated;
    }

    private void OnDestroy()
    {
        Plugin.GlobalSaveData.phaseGateIndices.Remove(prefabIdentifier.Id);
        Plugin.GlobalSaveData.phaseGateLocations.Remove(prefabIdentifier.Id);
    }

    private void OnStartedSaving(object sender, JsonFileEventArgs args)
    {
        Plugin.GlobalSaveData.phaseGateIndices[prefabIdentifier.Id] = gateIndex;
    }

    public void OnProtoSerializeObjectTree(ProtobufSerializer serializer) { }

    public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
    {
        if (!Plugin.GlobalSaveData.phaseGateIndices.TryGetValue(prefabIdentifier.Id, out gateIndex)) return;
        
        UpdateConnectedGate();
    }
}