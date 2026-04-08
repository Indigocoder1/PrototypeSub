using PrototypeSubMod.IonGenerator;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.PrecursorWearables;
using PrototypeSubMod.Prefabs.Factors;
using PrototypeSubMod.Registration;
using RootMotion;
using SubLibrary.Audio;
using System;
using System.Collections;
using PrototypeSubMod.Facilities.Interceptor;
using UnityEngine;

namespace PrototypeSubMod.Factors.Tether;

public class MarkerTetherLogic : Factor
{
    [SerializeField] private InterfloorTeleporter interfloorTeleporter;
    [SerializeField] private FMOD_CustomEmitter tetherPlaceSFX;
    [SerializeField] private FMOD_CustomEmitter noPowerSFX;
    [SerializeField] private float powerConsumption = 10f;
    [SerializeField] private float maxDistFromTether = 1000;

    public static event Action onClearTetherMarker;

    private GameObject warpInFx;

    public override GameInput.Button GetUseButton() => InputRegisterer.TetherMarkerButton;

    private IEnumerator Initialize()
    {
        var task = CraftData.GetPrefabForTechTypeAsync(TechType.Warper);
        yield return task;

        var result = task.GetResult();
        var warper = result.GetComponent<Warper>();
        warpInFx = warper.warpInEffectPrefab;
    }

    public override void StartUse()
    {
        if (InterceptorReactorSequenceManager.SequenceInProgress)
        {
            ErrorMessage.AddError(Language.main.Get("TetherUnavailable"));
            return;
        }
        
        if (Player.main.isPiloting) return;
        if (Player.main.precursorOutOfWater && Plugin.GlobalSaveData.tetherFactorMarkerLocation == null) return;
        if (Player.main.cinematicModeActive) return;
        if (Player.main.pda.isOpen) return;
        if (Player.main.currentSub != null) return;
        if (DevConsole.instance.state) return;

        UWE.CoroutineHost.StartCoroutine(Initialize());

        base.StartUse();
        if (Plugin.GlobalSaveData.tetherFactorMarkerLocation == null)
        {
            Plugin.GlobalSaveData.tetherFactorMarkerLocation = Player.main.transform.position;
            UWE.CoroutineHost.StartCoroutine(SpawnMarker(Player.main.transform.position));
            ErrorMessage.AddError(Language.main.Get("TetherFactorPlaced"));
            tetherPlaceSFX.Play();
            return;
        }

        var itemInSlot = Inventory.main.equipment.GetItemInSlot("Body");
        FactorIonManager ionManager = itemInSlot.item.GetComponent<FactorIonManager>();

        if (ionManager.GetCurrentEnergy() < powerConsumption)
        {
            ErrorMessage.AddError(Language.main.Get("TetherFactorNoPower"));
            noPowerSFX.Play();
            return;
        }

        if (Vector3.Distance(Plugin.GlobalSaveData.tetherFactorMarkerLocation.Value, Player.main.transform.position) >
            maxDistFromTether)
        {
            ErrorMessage.AddError(Language.main.Get("TetherFactorTooFar"));
            return;
        }
        
        ionManager.ConsumeEnergy(powerConsumption);
        Player.main.SetPrecursorOutOfWater(false);

        Instantiate(warpInFx, Player.main.transform.position, Player.main.transform.rotation);
        UWE.CoroutineHost.StartCoroutine(TeleportPlayer(Plugin.GlobalSaveData.tetherFactorMarkerLocation.Value));
        Plugin.GlobalSaveData.tetherFactorMarkerLocation = null;

        onClearTetherMarker?.Invoke();
    }

    private IEnumerator TeleportPlayer(Vector3 position)
    {
        var isLoaded = IsAreaLoaded(position, LargeWorldEntity.CellLevel.Medium);
        foreach (var moonpoolTrigger in FindObjectsOfType<PrecursorMoonPoolTrigger>())
        {
            moonpoolTrigger.OnTriggerExit(Player.mainCollider);
        }

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