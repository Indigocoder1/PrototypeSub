using System;
using System.Collections;
using System.Linq;
using Nautilus.Utility;
using PrototypeSubMod.DeployablesTerminal;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using PrototypeSubMod.Prefabs;
using PrototypeSubMod.UI.AbilitySelection;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.PhaseGates;

public class PhaseGateSubAbility : MonoBehaviour, IAbilityIcon
{
    internal static event Action onPhaseGateCreated; 
    
    [SaveStateReference]
    private static GameObject _phaseGatePrefab;
    
    [SaveStateReference]
    private static GameObject _phaseGateItemPrefab;
    
    [SerializeField] private DeployablesStorageTerminal storageTerminal;
    [SerializeField] private SelectionMenuManager selectionMenuManager;
    [SerializeField] private Sprite constructIcon;
    [SerializeField] private Sprite deconstructIcon;
    [SerializeField] private Vector3 localGhostOffset;
    [SerializeField] private BoxCollider[] checkBounds;
    [SerializeField] private float timeToConstruct;
    [SerializeField] private float timeToDeconstruct = 30;
    
    [Header("Voicelines")]
    [SerializeField] private VoiceNotificationManager voiceNotificationManager;
    [SerializeField] private VoiceNotification alphaDeployed;
    [SerializeField] private VoiceNotification betaDeployed;
    [SerializeField] private VoiceNotification alphaOnline;
    [SerializeField] private VoiceNotification betaOnline;
    [SerializeField] private VoiceNotification alphaOffline;
    [SerializeField] private VoiceNotification betaOffline;
    [SerializeField] private VoiceNotification alphaLoaded;
    [SerializeField] private VoiceNotification betaLoaded;
    [SerializeField] private VoiceNotification confirmDeconstruction;
    [SerializeField] private VoiceNotification noGateDetected;
    [SerializeField] private VoiceNotification maxGatesPlaced;
    [SerializeField] private VoiceNotification noDeploymentSpace;
    
    private GameObject ghostObject;
    private Material ghostMaterial;
    private int checkLayerMask;
    private bool selected;
    private bool constructing;
    private bool deconstructing;
    private bool deconstructRequested;
    private bool hadRoomForDeployment;

    private void Start()
    {
        UWE.CoroutineHost.StartCoroutine(GetPhaseGatePrefab());
        UWE.CoroutineHost.StartCoroutine(SpawnGhostObject());

        checkLayerMask = (1 << LayerID.TerrainCollider) | (1 << LayerID.BaseClipProxy) | (1 << LayerID.Vehicle) | (1 << LayerID.Useable);
        storageTerminal.equipment.onEquip += (_, _) =>
        {
            selectionMenuManager.RefreshIcons();
        };
        
        storageTerminal.equipment.onUnequip += (_, _) =>
        {
            selectionMenuManager.RefreshIcons();
            if (!HasPhaseGate())
            {
                ghostObject.SetActive(false);
            }
        };
    }

    private IEnumerator GetPhaseGatePrefab()
    {
        var task = CraftData.GetPrefabForTechTypeAsync(ProtoPhaseGate.PrefabInfo.TechType);
        yield return task;
        _phaseGatePrefab = task.result.value;

        var itemTask = CraftData.GetPrefabForTechTypeAsync(ProtoPhaseGateItem.PrefabInfo.TechType);
        yield return itemTask;
        _phaseGateItemPrefab = itemTask.result.value;
    }
    
    private IEnumerator SpawnGhostObject()
    {
        yield return new WaitUntil(() => _phaseGatePrefab);

        ghostObject = UWE.Utils.InstantiateDeactivated(_phaseGatePrefab, transform, localGhostOffset,
            Quaternion.identity, Vector3.one);

        DisplayCaseProp.TrimComponents(ghostObject, DisplayCaseProp.whitelistedComponents);

        var colliders = ghostObject.GetComponentsInChildren<Collider>(true);
        for (int i = colliders.Length - 1; i >= 0; i--)
        {
            Destroy(colliders[i]);
        }

        ghostMaterial = new Material(MaterialUtils.GhostMaterial);
        ghostMaterial.color = new Color(0.476f, 1f, 0.381f);
        ghostMaterial.SetColor(ShaderPropertyID._BorderColor, new Color(0.476f, 1f, 0.381f));
        foreach (var renderer in ghostObject.GetComponentsInChildren<Renderer>(true))
        {
            var newMaterials = Enumerable.Repeat(ghostMaterial, renderer.materials.Length).ToArray();
            renderer.materials = newMaterials;
        }

        selectionMenuManager.RefreshIcons();
    }
    
    private bool HasPhaseGate()
    {
        return storageTerminal.equipment.GetItemInSlot(DeployablesStorageTerminal.PHASE_GATE_SLOT) != null;
    }

    private void Update()
    {
        bool hasRoom = HasRoomForDeployment();
        if (hadRoomForDeployment != hasRoom)
        {
            selectionMenuManager.RefreshIcons();
        }
        
        hadRoomForDeployment = hasRoom;
        
        if (!selected || !HasPhaseGate()) return;

        var color = HasRoomForDeployment() ? new Color(0.476f, 1f, 0.381f) : new Color(1, 0.6835f, 0.0157f);
        ghostMaterial.color = color;
        ghostMaterial.SetColor(ShaderPropertyID._BorderColor, color);
    }

    public bool OnActivated()
    {
        if (deconstructing)
        {
            ErrorMessage.AddError($"Currently deconstructing!");
            return false;
        }

        if (constructing)
        {
            ErrorMessage.AddError($"Currently constructing!");
            return false;
        }

        if (HasPhaseGate())
        {
            return HandleNewPhaseGates();
        }
        
        return HandleGateDeconstruction();
    }

    private bool HandleNewPhaseGates()
    {
        if (Plugin.GlobalSaveData.phaseGateLocations.Count >= 2)
        {
            voiceNotificationManager.PlayVoiceNotification(maxGatesPlaced);
            return false;
        }
        
        if (!HasRoomForDeployment())
        {
            voiceNotificationManager.PlayVoiceNotification(noDeploymentSpace);
            return false;
        }
        
        var gateInstance = Instantiate(_phaseGatePrefab, ghostObject.transform.position, ghostObject.transform.rotation);
        storageTerminal.equipment.RemoveItem(DeployablesStorageTerminal.PHASE_GATE_SLOT, false, false);

        ghostObject.SetActive(false);
        var fxSpawner = gateInstance.transform.Find("Gate/Teleporter/FXSpawnPos");
        for (int i = fxSpawner.childCount - 1; i >= 0; i--)
        {
            Destroy(fxSpawner.GetChild(i).gameObject);
        }

        var gateLocation = new PhaseGateLocation(ghostObject.transform.position, -ghostObject.transform.forward);
        var identifier = gateInstance.GetComponent<PrefabIdentifier>();
        Plugin.GlobalSaveData.phaseGateLocations.Add(identifier.Id, gateLocation);

        var gateIndices = Plugin.GlobalSaveData.phaseGateIndices;
        int lastIndex = -1;
        if (gateIndices.Count > 0)
        {
            lastIndex = gateIndices.ElementAt(Plugin.GlobalSaveData.phaseGateIndices.Count - 1).Value;
        }

        int newIndex = lastIndex + 1;
        var gateManager = gateInstance.GetComponent<ProtoPhaseGateManager>();
        gateManager.SetGateIndex(newIndex % 2);
        gateManager.OnConstructed();

        var vfxConstructing = gateInstance.GetComponent<VFXConstructing>();
        vfxConstructing.ghostMaterial = MaterialUtils.GhostMaterial;
        vfxConstructing.timeToConstruct = timeToConstruct;
        vfxConstructing.informGameObject = gameObject;
        vfxConstructing.StartConstruction();
 
        gateInstance.GetComponent<LargeWorldEntity>().enabled = true;
        
        onPhaseGateCreated?.Invoke();
        constructing = true;
        voiceNotificationManager.PlayVoiceNotification(newIndex % 2 == 0 ? alphaDeployed : betaDeployed);
        return true;
    }

    private bool HandleGateDeconstruction()
    {
        var gateManager = GetGateManagerInRange();
        if (!gateManager)
        {
            voiceNotificationManager.PlayVoiceNotification(noGateDetected);
            return false;
        }
        
        if (HasPhaseGate())
        {
            ErrorMessage.AddError("Phase gate storage full! Can't deconstruct");
            return false;
        }
        
        if (!deconstructRequested)
        {
            voiceNotificationManager.PlayVoiceNotification(confirmDeconstruction);
            CancelInvoke(nameof(ResetDeconstructRequest));
            Invoke(nameof(ResetDeconstructRequest), 1f);
            deconstructRequested = true;
            return false;
        }
        
        StartCoroutine(DeconstructGate(gateManager));
        deconstructRequested = false;
        voiceNotificationManager.PlayVoiceNotification(Plugin.GlobalSaveData.phaseGateIndices.Count % 2 == 0 ? betaOffline : alphaOffline);
        return true;
    }

    private ProtoPhaseGateManager GetGateManagerInRange()
    {
        var colliders = Physics.OverlapBox(checkBounds[0].transform.position, checkBounds[0].transform.localScale / 2, 
            checkBounds[0].transform.rotation, 1 << LayerID.Useable);
        
        ProtoPhaseGateManager phaseGateManager = null;
        foreach (var col in colliders)
        {
            phaseGateManager = col.GetComponentInParent<ProtoPhaseGateManager>();
            if (phaseGateManager)
            {
                break;
            }
        }

        return phaseGateManager;
    }

    private void ResetDeconstructRequest()
    {
        deconstructRequested = false;
    }

    private IEnumerator DeconstructGate(ProtoPhaseGateManager gateManager)
    {
        deconstructing = true;
        storageTerminal.gameObject.SetActive(false);
        gateManager.DeactivateGate();

        yield return new WaitForSeconds(0.75f);
        
        var vfxConstructing = gateManager.GetComponent<VFXConstructing>();
        vfxConstructing.ghostOverlay = vfxConstructing.gameObject.EnsureComponent<VFXOverlayMaterial>();
        vfxConstructing.ghostMaterial = new Material(MaterialUtils.GhostMaterial);
        vfxConstructing.ghostMaterial.color = gateManager.GetComponent<GhostMaterialSetter>().GetGhostColor();
        vfxConstructing.ghostOverlay.ApplyOverlay(vfxConstructing.ghostMaterial, "VFXDeconstructing", false);
        foreach (var renderer in gateManager.GetComponentsInChildren<Renderer>())
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

        var returnedItem = Instantiate(_phaseGateItemPrefab);
        var pickupable = returnedItem.GetComponent<Pickupable>();
        pickupable.Initialize();
        pickupable.Deactivate();
        pickupable.inventoryItem = new InventoryItem(pickupable);
        
        storageTerminal.equipment.AddItem(DeployablesStorageTerminal.PHASE_GATE_SLOT,
            pickupable.inventoryItem);
        uGUI_IconNotifier.main.Play(ProtoPhaseGateItem.PrefabInfo.TechType, uGUI_IconNotifier.AnimationType.From);

        Destroy(gateManager.gameObject);
        Destroy(vfxConstructing.ghostMaterial);
        storageTerminal.gameObject.SetActive(true);
        voiceNotificationManager.PlayVoiceNotification(Plugin.GlobalSaveData.phaseGateIndices.Count % 2 == 1 ? betaLoaded : alphaLoaded);

        deconstructing = false;
    }

    public void OnConstructionDone(GameObject sender)
    {
        constructing = false;
        
        voiceNotificationManager.PlayVoiceNotification(Plugin.GlobalSaveData.phaseGateIndices.Count % 2 == 0 ? alphaOnline : betaOnline);
        if (Plugin.GlobalSaveData.phaseGateIndices.Count % 2 != 0) return;

        sender.GetComponent<ProtoPhaseGateManager>().ActivateGate();
    }

    private bool HasRoomForDeployment()
    {
        foreach (var col in checkBounds)
        {
            var hit = Physics.CheckBox(col.transform.position, col.transform.localScale / 2, 
                col.transform.rotation, checkLayerMask);
            if (hit) return false;
        }

        return true;
    }

    private void OnDestroy()
    {
        Destroy(ghostMaterial);
    }

    public void OnSelectedChanged(bool changed)
    {
        if ((HasPhaseGate() || !changed) && !deconstructing)
        {
            ghostObject.SetActive(changed);
        }

        selected = changed;
    }

    public bool GetActive() => false;
    public bool GetCanActivate() => true;

    public bool GetShouldShow()
    {
        if (!_phaseGatePrefab) return false;

        return Plugin.GlobalSaveData.phaseGateLocations.Count >= 1 || HasPhaseGate();
    }

    public Sprite GetSprite()
    {
        return HasPhaseGate() ? constructIcon : deconstructIcon;
    }
    
    public TechType GetTechType() => TechType.None;
}