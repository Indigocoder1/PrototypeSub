using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Utility;
using PrototypeSubMod.DeployablesTerminal;
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
    
    [SerializeField] private DeployablesStorageTerminal storageTerminal;
    [SerializeField] private SelectionMenuManager selectionMenuManager;
    [SerializeField] private Sprite radialIcon;
    [SerializeField] private Vector3 localGhostOffset;
    [SerializeField] private BoxCollider checkBounds;

    private GameObject ghostObject;
    private int phaseGateItemCount;
    private int checkLayerMask;
    private readonly List<string> availableLightSlots = new();

    private void Start()
    {
        UWE.CoroutineHost.StartCoroutine(GetPhaseGatePrefab());
        UWE.CoroutineHost.StartCoroutine(SpawnGhostObject());

        checkLayerMask = (1 << LayerID.TerrainCollider) | (1 << LayerID.BaseClipProxy) | (1 << LayerID.Vehicle) | (1 << LayerID.Useable);
        storageTerminal.equipment.onEquip += (slot, item) =>
        {
            if (item.techType == ProtoPhaseGateStabilizer.PrefabInfo.TechType)
            {
                selectionMenuManager.RefreshIcons();
            }
        };
    }

    private IEnumerator GetPhaseGatePrefab()
    {
        var task = CraftData.GetPrefabForTechTypeAsync(ProtoPhaseGate.PrefabInfo.TechType);
        yield return task;
        _phaseGatePrefab = task.result.value;
    }
    
    private IEnumerator SpawnGhostObject()
    {
        yield return new WaitUntil(() => _phaseGatePrefab);

        ghostObject = UWE.Utils.InstantiateDeactivated(_phaseGatePrefab, transform, localGhostOffset,
            Quaternion.identity, Vector3.one);

        Destroy(ghostObject.GetComponent<LargeWorldEntity>());
        Destroy(ghostObject.GetComponent<PrefabIdentifier>());

        var colliders = ghostObject.GetComponentsInChildren<Collider>(true);
        for (int i = colliders.Length - 1; i >= 0; i--)
        {
            Destroy(colliders[i]);
        }
        
        foreach (var renderer in ghostObject.GetComponentsInChildren<Renderer>(true))
        {
            var newMaterials = Enumerable.Repeat(MaterialUtils.GhostMaterial, renderer.materials.Length).ToArray();
            renderer.materials = newMaterials;
        }

        selectionMenuManager.RefreshIcons();
    }
    
    private void RecalculateDeployableTotals()
    {
        phaseGateItemCount = 0;
        availableLightSlots.Clear();

        foreach (var slot in DeployablesStorageTerminal.LightBeaconSlots)
        {
            var item = storageTerminal.equipment.GetItemInSlot(slot);

            if (item != null && item.techType == ProtoPhaseGateStabilizer.PrefabInfo.TechType)
            {
                availableLightSlots.Add(slot);
                phaseGateItemCount++;
            }
        }
    }

    public bool OnActivated()
    {
        if (phaseGateItemCount == 0)
        {
            ErrorMessage.AddError("No phase gates loaded in launch bay!");
            return false;
        }

        if (Plugin.GlobalSaveData.phaseGateLocations.Count >= 2)
        {
            ErrorMessage.AddError("Two gates already constructed!");
            return false;
        }

        bool hitObject = Physics.CheckBox(checkBounds.transform.position, checkBounds.transform.localScale / 2, checkBounds.transform.rotation, checkLayerMask);
        if (hitObject)
        {
            ErrorMessage.AddError("Not enough room for deployment!");
            return false;
        }
        
        var gateInstance = Instantiate(_phaseGatePrefab, ghostObject.transform.position, ghostObject.transform.rotation);
        storageTerminal.equipment.RemoveItem(availableLightSlots[0], false, false);
        RecalculateDeployableTotals();

        if (phaseGateItemCount <= 0)
        {
            ghostObject.SetActive(false);
        }

        Plugin.GlobalSaveData.phaseGateLocations.Add(new PhaseGateLocation(ghostObject.transform.position, -ghostObject.transform.forward));

        var gateIndices = Plugin.GlobalSaveData.phaseGateIndices;
        int lastIndex = -1;
        if (gateIndices.Count > 0)
        {
            lastIndex = gateIndices.ElementAt(Plugin.GlobalSaveData.phaseGateIndices.Count - 1).Value;
        }

        int newIndex = lastIndex + 1;
        var gateManager = gateInstance.GetComponent<ProtoPhaseGateManager>();
        gateManager.SetGateIndex(newIndex % 2);
        if (newIndex % 2 == 1)
        {
            StartCoroutine(ActivateGateDelayed(gateManager));
        }
        
        
        onPhaseGateCreated?.Invoke();
        return true;
    }

    private IEnumerator ActivateGateDelayed(ProtoPhaseGateManager gateManager)
    {
        yield return new WaitForSeconds(1);
        
        gateManager.ActivateGate();
    }

    public void OnSelectedChanged(bool changed)
    {
        RecalculateDeployableTotals();
        if (phaseGateItemCount > 0)
        {
            ghostObject.SetActive(changed);
        }
    }

    public bool GetActive()
    {
        return false;
    }

    public bool GetCanActivate() => true;

    public bool GetShouldShow() => _phaseGatePrefab;

    public Sprite GetSprite() => radialIcon;
    public TechType GetTechType() => TechType.None;
}