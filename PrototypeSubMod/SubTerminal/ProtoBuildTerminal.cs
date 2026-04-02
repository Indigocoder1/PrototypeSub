using System;
using PrototypeSubMod.Prefabs;
using Story;
using System.Collections;
using Nautilus.Utility;
using PrototypeSubMod.DestructionEvent;
using PrototypeSubMod.IonBarrier;
using PrototypeSubMod.LightDistortionField;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.PowerSystem;
using PrototypeSubMod.PressureConverters;
using PrototypeSubMod.Teleporter;
using PrototypeSubMod.UI.HealthDisplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrototypeSubMod.SubTerminal;

internal class ProtoBuildTerminal : Crafter
{
    [SerializeField] private float buildDuration = 20f;
    [SerializeField] private float buildDelay;
    [SerializeField] private Transform safeReturnPos;
    [SerializeField] private FMODAsset buildSoundEffect;
    [SerializeField] private FMOD_CustomEmitter chargeUpSFX;
    [SerializeField] private FMOD_CustomEmitter dischargeSFX;
    [SerializeField] private FMOD_CustomEmitter constructSfx;
    [SerializeField] private Transform buildPosition;
    [SerializeField] private GameObject upgradeIconPrefab;
    [SerializeField] private ProtoBatteryManager[] batteryManagers;
    [SerializeField] private ProtoBuildBot[] buildBots;
    [SerializeField] private Animator spikesAnimator;
    [SerializeField] private WarpInFXPlayer warpFXSpawner;
    [SerializeField] private SubReconstructionManager reconstructionManager;
    [SerializeField] private string buildStartPdaKey;
    [SerializeField] private string buildFinishPdaKey;
    
    [Header("Screens")]
    [SerializeField] private BuildTerminalScreenManager screenManager;
    [SerializeField] private uGUI_BuildAnimScreen animScreen;

    private MoonpoolOccupiedHandler occupiedHandler;
    private int returnedBotCount;

    private void Start()
    {
        occupiedHandler = GetComponentInChildren<MoonpoolOccupiedHandler>();
    }
    
    public void CraftSub()
    {
        if (!MoonpoolCanHavePrototype())
        {
            ErrorMessage.AddError(Language.main.Get("BuildTerminal_Occupied"));
            return;
        }
        
        Craft(Prototype_Craftable.SubInfo.TechType, buildDuration);
    }

    public override void Craft(TechType techType, float duration)
    {
        if (!CrafterLogic.ConsumeResources(techType)) return;

        UWE.CoroutineHost.StartCoroutine(StartCraftChargeUp(duration));
        UWE.CoroutineHost.StartCoroutine(StartReconstruction(reconstructionManager));
        UWE.CoroutineHost.StartCoroutine(PlayConstructSfxDelayed());
        StoryGoalManager.main.OnGoalComplete("PrototypeCrafted");

        PDALog.Add(buildStartPdaKey);
    }

    private IEnumerator PlayConstructSfxDelayed()
    {
        yield return new WaitForSeconds(buildDuration - 4);
        
        constructSfx.Play();
    }

    public void RebuildSub()
    {
        if (!MoonpoolCanHavePrototype())
        {
            ErrorMessage.AddError(Language.main.Get("BuildTerminal_Occupied"));
            return;
        }
        
        UWE.CoroutineHost.StartCoroutine(StartCraftChargeUp(buildDuration));
        UWE.CoroutineHost.StartCoroutine(StartReconstruction(reconstructionManager));
        UWE.CoroutineHost.StartCoroutine(PlayConstructSfxDelayed());
    }
    
    public void RecentralizeSub()
    {
        if (!MoonpoolCanHavePrototype())
        {
            ErrorMessage.AddError(Language.main.Get("BuildTerminal_Occupied"));
            return;
        }
        
        UWE.CoroutineHost.StartCoroutine(StartCraftChargeUp(buildDuration));
        UWE.CoroutineHost.StartCoroutine(RecentralizeSubDelayed());
    }

    private IEnumerator StartCraftChargeUp(float duration)
    {
        chargeUpSFX.Play();
        StartCoroutine(PlayDischargeDelayed());
        screenManager.BeginBuildStage();
        spikesAnimator.SetTrigger("BuildWarmup");
        animScreen.StartPreWarm(buildDelay);

        foreach (var item in batteryManagers)
        {
            item.StartBatteryCharge(buildDelay);
        }

        yield return new WaitForSeconds(buildDelay);
        
        foreach (var item in batteryManagers)
        {
            item.StartBatteryDrain(duration);
        }

        if (occupiedHandler.GetBounds().Contains(Player.main.transform.position))
        {
            UWE.CoroutineHost.StartCoroutine(TeleportPlayerOut());
        }
    }
    
    private void StartConstruction(GameObject instantiatedPrefab, TechType techType, float duration)
    {
        screenManager.OnConstructionStarted();
        FMODUWE.PlayOneShot(buildSoundEffect, buildPosition.position);

        CrafterLogic.NotifyCraftEnd(instantiatedPrefab, techType);
        ItemGoalTracker.OnConstruct(techType);
        var vfxConstructing = instantiatedPrefab.GetComponent<VFXConstructing>();
        if (!vfxConstructing) throw new Exception($"No VFXConstructing component on {instantiatedPrefab}");
            
        vfxConstructing.enabled = true;
        vfxConstructing.timeToConstruct = duration;
        vfxConstructing.StartConstruction();
        vfxConstructing.informGameObject = gameObject;

        animScreen.StartAnimation(duration + vfxConstructing.delay);

        LargeWorldEntity.Register(instantiatedPrefab);
        SendBuildBots(instantiatedPrefab);
        Plugin.GlobalSaveData.prototypePresent = true;
    }

    private IEnumerator StartReconstruction(SubReconstructionManager manager)
    {
        yield return new WaitForSeconds(buildDelay);

        screenManager.OnConstructionStarted();
        manager.OnConstructionStarted(buildPosition.position, buildPosition.rotation);
        var sub = manager.GetSubObject();
        sub.transform.position = buildPosition.position;
        sub.transform.rotation = buildPosition.rotation;
        sub.gameObject.SetActive(true);
        warpFXSpawner.SpawnWarpInFX(buildPosition.position, Vector3.one * 2f);
        sub.GetComponentInChildren<ProtoDestructionEvent>().OnRebuilt();

        var hydrolockAnimator = sub.GetComponentInChildren<ProtoIonBarrier>().GetComponentInChildren<Animator>();
        hydrolockAnimator.gameObject.SetActive(false);
        
        yield return new WaitForEndOfFrame();
        var constructing = sub.GetComponent<VFXConstructing>();
        constructing.ghostMaterial = MaterialUtils.GhostMaterial;
        constructing.delay = 2;
        yield return new WaitForEndOfFrame();
        
        hydrolockAnimator.gameObject.SetActive(true);
        
        StartConstruction(sub, TechType.None, buildDuration);

        var subRoot = sub.GetComponent<SubRoot>();
        subRoot.subDestroyed = false;
        subRoot.worldForces.underwaterGravity = 0;
        sub.GetComponent<Stabilizer>().enabled = true;
        sub.GetComponent<PingInstance>().enabled = true;
        sub.GetComponentInChildren<ProtoHealthDisplay>().UpdateHealth();
        sub.GetComponent<ProtoRigidbodyFreezer>().SendMessage("FixedUpdate");
        foreach (var interfloorTeleporter in sub.GetComponentsInChildren<InterfloorTeleporter>(true))
        {
            var col = interfloorTeleporter.GetComponent<Collider>();
            if (!col) continue;
            col.enabled = true;
        }
        var teleporter = sub.GetComponentInChildren<ProtoTeleporterManager>();
        teleporter.transform.Find("FXSpawn").gameObject.SetActive(true);
        teleporter.transform.Find("ActivationCanvas").gameObject.SetActive(teleporter.GetUpgradeInstalled());
        teleporter.transform.Find("ActivationCanvas (1)").gameObject.SetActive(teleporter.GetUpgradeInstalled());

        sub.GetComponentInChildren<PrototypePowerSystem>().UpdateRelayStatus();
        
        // Failsafe end construct to fix Octo's weird bug
        yield return new WaitForSeconds(buildDuration + 1f);
        
        if (constructing.isDone && constructing.enabled)
        {
            constructing.RevertMaterials();
            constructing.WakeUpSubmarine();
            constructing.informGameObject.BroadcastMessage("OnConstructionDone", constructing.gameObject);
            constructing.EndConstruct();
        }
        
        PDALog.Add(buildFinishPdaKey);
    }

    private IEnumerator RecentralizeSubDelayed()
    {
        yield return new WaitForSeconds(buildDelay);

        if (CloakEffectHandler.EffectHandlers.Count == 0) throw new Exception("No subs in scene to recentralize");

        var root = CloakEffectHandler.EffectHandlers[0].GetComponentInParent<SubRoot>();
        root.transform.position = buildPosition.position;
        root.transform.rotation = buildPosition.rotation;
        warpFXSpawner.SpawnWarpInFX(buildPosition.position, Vector3.one * 2f);
        screenManager.EndBuildStage();
    }

    private IEnumerator TeleportPlayerOut()
    {
        InterfloorTeleporter.PlayTeleportEffect(0.4f);

        yield return new WaitForSeconds(0.2f);

        Player.main.transform.position = safeReturnPos.position;
        Player.main.transform.rotation = safeReturnPos.rotation;
    }

    private bool MoonpoolCanHavePrototype()
    {
        var bounds = occupiedHandler.GetBounds();
        var transform = occupiedHandler.GetTransform();
        var objects = Physics.OverlapBox(bounds.center, bounds.extents, transform.rotation);
        bool moonpoolOccupied = false;
        foreach (var obj in objects)
        {
            if (obj.GetComponentInParent<Vehicle>())
            {
                moonpoolOccupied = true;
                break;
            }

            var isPrototype = obj.GetComponentInParent<ProtoPowerRelayManager>();
            
            if (obj.GetComponentInParent<SubRoot>() && !isPrototype)
            {
                moonpoolOccupied = true;
                break;
            }
        }

        return !moonpoolOccupied;
    }

    private IEnumerator PlayDischargeDelayed()
    {
        yield return new WaitForSeconds(10.6f);
        dischargeSFX.Play();
    }

    private void SendBuildBots(GameObject toBuild)
    {
        returnedBotCount = 0;

        var botPaths = toBuild.GetComponentsInChildren<BuildBotPath>();
        if (botPaths.Length == 0)
        {
            Plugin.Logger.LogError($"No bot paths found on {toBuild}");
            return;
        }

        spikesAnimator.enabled = false;

        for (int i = 0; i < buildBots.Length; i++)
        {
            int index = i % botPaths.Length;
            buildBots[i].SetPath(botPaths[index], toBuild);
        }
    }

    // Called by VFX Constructing
    public void OnConstructionDone(GameObject constructedObject)
    {
        for (int i = 0; i < buildBots.Length; i++)
        {
            buildBots[i].FinishConstruction(OnBotReturned);
        }

        screenManager.EndBuildStage();
    }

    private void OnBotReturned()
    {
        returnedBotCount++;
        if (returnedBotCount >= buildBots.Length)
        {
            spikesAnimator.enabled = true;
            foreach (var item in buildBots)
            {
                item.OnAllBotsReturned();
            }
        }
    }
}
