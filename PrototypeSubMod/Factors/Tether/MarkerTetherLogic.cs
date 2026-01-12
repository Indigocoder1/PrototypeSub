using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.PrecursorWearables;
using PrototypeSubMod.Prefabs.Factors;
using PrototypeSubMod.Registration;
using SubLibrary.Audio;
using System;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Factors.Tether;

public class MarkerTetherLogic : Factor
{
    [SerializeField] private InterfloorTeleporter interfloorTeleporter;
    [SerializeField] private FMOD_CustomEmitter tetherPlaceSFX;

    private float maxDistFromTether = 1000;
    private PrecursorSuitManager suitManager;

    public static event Action onClearTetherMarker;

    public override GameInput.Button GetUseButton() => InputRegisterer.TetherMarkerButton;

    public override void StartUse()
    {
        if (Player.main.isPiloting) return;
        if (Player.main.precursorOutOfWater && Plugin.GlobalSaveData.tetherFactorMarkerLocation == null) return;
        if (Player.main.cinematicModeActive) return;
        if (Player.main.pda.isOpen) return;
        if (Player.main.currentSub != null) return;
        if (DevConsole.instance.state) return;
         
        base.StartUse();
        if (Plugin.GlobalSaveData.tetherFactorMarkerLocation == null)
        {
            Plugin.GlobalSaveData.tetherFactorMarkerLocation = Player.main.transform.position;
            Plugin.GlobalSaveData.tetherMarkerOutOfWater = Player.main.precursorOutOfWater;
            UWE.CoroutineHost.StartCoroutine(SpawnMarker(Player.main.transform.position));
            ErrorMessage.AddError("Tether marker placed. Use again to teleport to marker");
            tetherPlaceSFX.Play();
            return;
        }

        if (Vector3.Distance(Plugin.GlobalSaveData.tetherFactorMarkerLocation.Value, Player.main.transform.position) >
            maxDistFromTether)
        {
            ErrorMessage.AddError("Too far from tether!");
            return;
        }
        
        UWE.CoroutineHost.StartCoroutine(TeleportPlayer(Plugin.GlobalSaveData.tetherFactorMarkerLocation.Value));
        Plugin.GlobalSaveData.tetherFactorMarkerLocation = null;
        onClearTetherMarker?.Invoke();

        var itemInSlot = Inventory.main.equipment.GetItemInSlot("Body");
        FactorIonManager ionManager = itemInSlot.item.GetComponent<FactorIonManager>();

        ionManager.ConsumeEnergy(10f);

    }

    private IEnumerator TeleportPlayer(Vector3 position)
    {
        var isLoaded = IsAreaLoaded(position, LargeWorldEntity.CellLevel.Medium);
        foreach (var moonpoolTrigger in FindObjectsOfType<PrecursorMoonPoolTrigger>())
        {
            moonpoolTrigger.OnTriggerExit(Player.mainCollider);
        }
        Player.main.SetPrecursorOutOfWater(Plugin.GlobalSaveData.tetherMarkerOutOfWater);

        if (isLoaded)
        {
            interfloorTeleporter.StartTeleportPlayer(position, Camera.main.transform.forward);
            yield return new WaitForSeconds(0.5f);
            Player.main.SetDisplaySurfaceWater(true);
            yield break;
        }
        
        var subTetherLogic = GetComponent<SubTetherLogic>();
        UWE.CoroutineHost.StartCoroutine(
            subTetherLogic.TeleportToLocation(Plugin.GlobalSaveData.tetherFactorMarkerLocation.Value, 0));
        yield return new WaitForSeconds(0.5f);
        Player.main.SetDisplaySurfaceWater(true);
    }

    private bool IsAreaLoaded(Vector3 position, LargeWorldEntity.CellLevel cellLevel)
    {
        return LargeWorldStreamer.main.cellManager.AreCellsLoaded(new Bounds(position,Vector3.one * 0.1f), cellLevel);
    }

    private IEnumerator SpawnMarker(Vector3 position)
    {
        var prefabTask = CraftData.GetPrefabForTechTypeAsync(TetherFactorMarker.prefabInfo.TechType);
        yield return prefabTask;
        var prefab = prefabTask.GetResult();
        var instance = Instantiate(prefab);
        instance.transform.position = position;
    }

}