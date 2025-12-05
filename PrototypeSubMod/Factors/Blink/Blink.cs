using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Handlers;
using PrototypeSubMod.Patches;
using PrototypeSubMod.PrecursorWearables;
using PrototypeSubMod.Prefabs;
using SubLibrary.Handlers;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace PrototypeSubMod.Factors.Blink;

public class Blink : Factor
{
    public Blink()
    {
        cooldown = 1f;
    }

    [SerializeField] private FMOD_CustomEmitter startSfx;
    [SerializeField] private FMOD_CustomEmitter stopSfx;
    [SerializeField] private FMOD_CustomLoopingEmitter loopingSfx;
    [SerializeField] private FMOD_CustomEmitter stopDueToChargeSfx;
    [SerializeField] private FMOD_CustomEmitter rechargeLoopSfx;
    [SerializeField] private FMOD_CustomEmitter rechargeFinishedSfx;
    [SerializeField] private AnimationCurve frequencyOverCharge;
    [SerializeField] private AnimationCurve rechargeVolumeOverTime;
    [SerializeField] private AnimationCurve loopVolumeOverTime;
    
    private float speedMultiplier = 3.5f;
    private float timeScaleSlow = 0.25f;
    
    private float maxBlinkDuration = 2f;
    private float blinkRechargeRate = 0.3f;
    private float ionEnergyPerResource = 1f;

    private float timeBetweenGhostFrames = 0.1f;
    private float ghostDeletionDelay = 2f;
    private float timeBetweenGhostDeletions = 0.1f;
    private float ghostFadeTime = 0.1f;
    private float timeNextDeleteGhost;

    private float chromaticAbberationVal = 3f;
    private float depthOfFieldVal = 0.1f;
    private float fovMultiplier = 1.5f;
    private float fovTransitionTime = 0.1f;
    
    private float resourceRegenDelay = 2f;
    [HideInInspector] public float resourceBarFadeDelay = 1f;
    [HideInInspector] public float resourceFadeInTime = 0.2f;
    [HideInInspector] public float resourceFadeOutTime = 0.5f;

    private readonly List<GameObject> ghostFrames = new();
    
    private PrecursorSuitManager suitManager;
    private ChromaticAberrationModel.Settings originalChromaticSettings;
    private DepthOfFieldModel.Settings originalDepthOfFieldSettings;
    private PlayerController controller;
    private PDACameraFOVControl pdaCameraControl;
    private Coroutine timescaleCoroutine;
    private BlinkResourceUI resourceUi;
    private FactorIonManager ionManager;
    private SpeedData speedData;
    private Material ghostMaterial;
    private float timeStartResourceRegen;
    private float timeStartedBlink;
    private float currentBlinkResource;
    private float blinkResourceLastFrame;
    private float timeNextGhostFrame;
    private bool wasChromaticActive;
    private bool stoppedDueToCharge;

    private void Awake()
    {
        UWE.CoroutineHost.StartCoroutine(GetGhostMaterial());
        pdaCameraControl = Player.main.GetComponent<PDACameraFOVControl>();
        suitManager = Player.main.GetComponent<PrecursorSuitManager>();
        Inventory.main.equipment.onEquip += RefreshIonManager;
    }

    private IEnumerator GetGhostMaterial()
    {
        yield return CyclopsReferenceHandler.EnsureCyclopsReference();
        
        var ghostBorder =
            CyclopsReferenceHandler.CyclopsReference.transform.Find(
                "HolographicDisplay/HolographicDisplayVisuals/CyclopsMini_Mid/border");
        ghostMaterial = new(ghostBorder.GetComponent<Renderer>().material);
        ghostMaterial.color = new Color(0.443f, 1, 0.443f);
    }

    public override void StartUse()
    {
        if (currentBlinkResource <= 0) return;
        
        if (Player.main.precursorOutOfWater || Player.main.transform.position.y > 0) return;
        if (Player.main.isPiloting || Player.main.pda.isOpen) return;
        if (Player.main.currentSub != null) return;
        if (Player.main.cinematicModeActive) return;
        
        controller = Player.main.playerController;

        if (controller == null)
        {
            Plugin.Logger.LogError("Failed to get Motor from Player for the BlinkFactor!");
            return;
        }

        base.StartUse();

        startSfx.Play();
        loopingSfx.Play();
        rechargeLoopSfx.Stop();
        suitManager.RegisterEmissionController(this, new PrecursorSuitManager.EmissionController(Color.green, 1));
        
        RefreshIonManager(null, null);
        speedData.CopyFromController(controller);
        speedData.Multiply(speedMultiplier / timeScaleSlow);
        speedData.AssignToMotor(controller.underWaterController);
        var moveDir = MainCameraControl.main.transform.right * GameInput.moveDirection.normalized.x +
            MainCameraControl.main.transform.forward * GameInput.moveDirection.normalized.z +
            MainCameraControl.main.transform.up * GameInput.moveDirection.normalized.y;
        Player.main.rigidBody.velocity = moveDir * (controller.swimForwardMaxSpeed * (speedMultiplier / timeScaleSlow));

        if (timescaleCoroutine != null)
        {
            UWE.CoroutineHost.StopCoroutine(timescaleCoroutine);
        }
        
        timescaleCoroutine = UWE.CoroutineHost.StartCoroutine(SetTimescaleDelayed(timeScaleSlow));
        PlayerController_Patches.SetBlockMotorModeAssignment(true);

        float targetFOV = Mathf.Min(MiscSettings.fieldOfView * fovMultiplier, 100);
        UWE.CoroutineHost.StartCoroutine(LerpFOV(targetFOV, fovTransitionTime));
        var postProcessing = SNCameraRoot.main.mainCam.GetComponent<PostProcessingBehaviour>();
        originalChromaticSettings = postProcessing.profile.chromaticAberration.settings;
        originalDepthOfFieldSettings = postProcessing.profile.depthOfField.settings;
        wasChromaticActive = postProcessing.profile.chromaticAberration.enabled;
        
        postProcessing.profile.chromaticAberration.enabled = true;
        var chromaticSettings = postProcessing.profile.chromaticAberration.settings;
        chromaticSettings.intensity = chromaticAbberationVal;
        postProcessing.profile.chromaticAberration.settings = chromaticSettings;
        
        var depthSettings = postProcessing.profile.depthOfField.settings;
        depthSettings.focusDistance = depthOfFieldVal;
        postProcessing.profile.depthOfField.settings = depthSettings;
        
        resourceUi.OpenUI(this);
        pdaCameraControl.enabled = false;
        
        timeNextDeleteGhost = Time.time + ghostDeletionDelay;
        timeStartedBlink = Time.unscaledTime;

        if (CustomSoundHandler.TryGetCustomSoundChannel(loopingSfx.GetInstanceID(), out var loopingChannel))
        {
            loopingChannel.setVolume(0);
        }
    }
    
    public override void StopUse()
    {
        base.StopUse();

        // Multiply by the inverse instead of dividing
        speedData.Multiply(timeScaleSlow / speedMultiplier);
        speedData.AssignToMotor(controller.underWaterController);
        PlayerController_Patches.SetBlockMotorModeAssignment(false);
        Time.timeScale = 1;
        Player.main.rigidBody.velocity = Player.main.rigidBody.velocity.normalized * GetCurrentMaxSpeed();
        Player.main.playerController.UpdateController();
        UWE.CoroutineHost.StartCoroutine(KeepPosAndUpdateMotorDelayed(Player.main.transform.position));

        suitManager.DeregisterEmissionController(this);
        SpawnGhostFrame();
        ResetEffects();
        timeStartResourceRegen = Time.time + resourceRegenDelay;

        loopingSfx.Stop();
        /*
        if (stoppedDueToCharge)
        {
            stopDueToChargeSfx.Play();
        }
        else
        {
            stopSfx.Play();
        }
        */

        stoppedDueToCharge = false;
    }

    private void ResetEffects()
    {
        UWE.CoroutineHost.StartCoroutine(LerpFOV(MiscSettings.fieldOfView, fovTransitionTime, () =>
        {
            pdaCameraControl.enabled = true;
        }));
        
        var postProcessing = SNCameraRoot.main.mainCam.GetComponent<PostProcessingBehaviour>();
        postProcessing.profile.chromaticAberration.enabled = wasChromaticActive;
        postProcessing.profile.chromaticAberration.settings = originalChromaticSettings;
        postProcessing.profile.depthOfField.settings = originalDepthOfFieldSettings;
    }

    private void RefreshIonManager(string _, InventoryItem __)
    {
        var itemInSlot = Inventory.main.equipment.GetItemInSlot("Body");
        if (itemInSlot == null) return;
        
        ionManager = itemInSlot.item.GetComponent<FactorIonManager>();
    }

    private IEnumerator KeepPosAndUpdateMotorDelayed(Vector3 position)
    {
        yield return null;
        Player.main.transform.position = position;
        Player.main.playerController.SetMotorMode(Player.main.motorMode);
    }

    private IEnumerator SetTimescaleDelayed(float timeScale)
    {
        // Wait until a FixedUpdate has ocurred to actually update the player velocity
        var timestepIncrements = (int)(Time.fixedUnscaledTime / Time.fixedUnscaledDeltaTime);
        while (timestepIncrements == (int)(Time.fixedUnscaledTime / Time.fixedUnscaledDeltaTime))
        {
            yield return null;
        }

        if (!inUse) yield break;

        Time.timeScale = timeScale;
    }

    private IEnumerator LerpFOV(float targetFOV, float time, Action onComplete = null)
    {
        float currentTime = 0;
        float initialFOV = SNCameraRoot.main.CurrentFieldOfView;
        while (currentTime < time)
        {
            SNCameraRoot.main.SetFov(CubicOut(initialFOV, targetFOV, currentTime / time));
            currentTime += Time.deltaTime;
            yield return null;
        }
        
        SNCameraRoot.main.SetFov(targetFOV);

        onComplete?.Invoke();
    }

    private float CubicOut(float start, float end, float time)
    {
        return start + (end - start) * (1 - Mathf.Pow(1 - time, 4));
    }

    private void RetrieveIndicatorReference()
    {
        if (resourceUi != null) return;
        
        var hudContent = uGUI.main.transform.Find("ScreenCanvas/HUD/Content");
        if (hudContent.Find("BlinkFactorCharge") == null)
        {
            var prefab = Plugin.AssetBundle.LoadAsset<GameObject>("BlinkFactorCharge");
            var instance = Instantiate(prefab, hudContent);
            instance.name = "BlinkFactorCharge";
            instance.transform.localPosition = new Vector3(-650, 400, 0);
            var hideForScreenshots = instance.EnsureComponent<HideForScreenshots>();
            hideForScreenshots.recursive = true;
        }
        
        resourceUi = hudContent.Find("BlinkFactorCharge").GetComponent<BlinkResourceUI>();
    }
    
    public override void UpdateFactor()
    {
        if (inUse && currentBlinkResource > 0)
        {
            currentBlinkResource -= Time.unscaledDeltaTime;
            HandleLoopSfx();
        }
        else if (currentBlinkResource < maxBlinkDuration && Time.time > timeStartResourceRegen && ionManager.GetCurrentEnergy() > 0)
        {
            currentBlinkResource += Time.deltaTime * blinkRechargeRate;
            ionManager.ConsumeEnergy(ionEnergyPerResource * Time.deltaTime);
            HandleRechargeSfx();
        }
        else if (rechargeLoopSfx.playing)
        {
            rechargeFinishedSfx.Play();
            rechargeLoopSfx.Stop();
        }

        if (Player.main.pda.isOpen && inUse)
        {
            StopUse();
        }
        
        if (currentBlinkResource <= 0 && inUse)
        {
            stoppedDueToCharge = true;
            StopUse();
        }

        if ((Player.main.precursorOutOfWater || Player.main.transform.position.y > 0) && inUse)
        {
            StopUse();
        }

        if (Mathf.Approximately(blinkResourceLastFrame, currentBlinkResource) && resourceUi.GetUIOpen() && Time.time > timeStartResourceRegen)
        {
            resourceUi.CloseUI(this);
        }

        if (resourceUi != null)
        {
            resourceUi.SetFillAmount(currentBlinkResource / maxBlinkDuration);
        }

        if (Time.time > timeNextGhostFrame && inUse)
        {
            timeNextGhostFrame = Time.time + timeBetweenGhostFrames * timeScaleSlow;
            SpawnGhostFrame();
        }
        
        if (Time.time > timeNextDeleteGhost && ghostFrames.Count > 0)
        {
            timeNextDeleteGhost = Time.time + timeBetweenGhostDeletions * timeScaleSlow;
            UWE.CoroutineHost.StartCoroutine(FadeOutGhost(ghostFrames[0]));
            ghostFrames.RemoveAt(0);
        }

        blinkResourceLastFrame = currentBlinkResource;
    }

    private void HandleRechargeSfx()
    {
        rechargeLoopSfx.Play();
        
        if (!CustomSoundHandler.TryGetCustomSoundChannel(rechargeLoopSfx.GetInstanceID(), out var loopingChannel)) return;

        loopingChannel.setFrequency(frequencyOverCharge.Evaluate(currentBlinkResource / maxBlinkDuration));
        loopingChannel.setVolume(rechargeVolumeOverTime.Evaluate(Mathf.Clamp01(Time.time - timeStartResourceRegen)));
    }

    private void HandleLoopSfx()
    {
        if (!CustomSoundHandler.TryGetCustomSoundChannel(loopingSfx.GetInstanceID(), out var loopingChannel)) return;
        
        loopingChannel.setVolume(loopVolumeOverTime.Evaluate(Mathf.Clamp01(Time.unscaledTime - timeStartedBlink)));
    }

    private IEnumerator FadeOutGhost(GameObject ghost)
    {
        var newMaterial = new Material(ghostMaterial);
        foreach (var rend in ghost.GetComponentsInChildren<Renderer>())
        {
            rend.materials = Enumerable.Repeat(newMaterial, rend.materials.Length).ToArray();
        }

        float fadeTime = 0;
        while (fadeTime < ghostFadeTime)
        {
            var col = newMaterial.color;
            col.a = 1 - (fadeTime / ghostFadeTime);
            newMaterial.color = col;
            fadeTime += Time.deltaTime;
            yield return null;
        }

        Destroy(ghost);
        Destroy(newMaterial);
    }

    private static readonly List<Type> WhitelistedTypes = new()
    {
        typeof(Transform),
        typeof(Renderer),
        typeof(MeshFilter),
        typeof(Animator)
    };
    
    private void SpawnGhostFrame()
    {
        var playerView = Player.main.transform.Find("body/player_view").gameObject;
        var newPlayerView = Instantiate(playerView, playerView.transform.position, playerView.transform.rotation);
        newPlayerView.name = "BlinkFactorGhost";
        ghostFrames.Add(newPlayerView.gameObject);
        DisplayCaseProp.TrimComponents(newPlayerView, WhitelistedTypes);
        var newAnim = newPlayerView.GetComponent<Animator>();
        var playerAnim = Player.main.playerAnimator;
        var handAttach =
            newPlayerView.transform.Find(
                "export_skeleton/head_rig/neck/chest/clav_R/clav_R_aim/shoulder_R/elbow_R/hand_R/attach1");

        foreach (Transform child in handAttach)
        {
            if (!child.name.StartsWith("attach1"))
            {
                Destroy(child.gameObject);
            }
        }
        
        foreach (var parameter in playerAnim.parameters)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Bool:
                    newAnim.SetBool(parameter.name, playerAnim.GetBool(parameter.name));
                    break;
                case AnimatorControllerParameterType.Float:
                    newAnim.SetFloat(parameter.name, playerAnim.GetFloat(parameter.name));
                    break;
                case AnimatorControllerParameterType.Int:
                    newAnim.SetInteger(parameter.name, playerAnim.GetInteger(parameter.name));
                    break;
            }
        }

        newAnim.Update(0);
        newAnim.enabled = false;
        
        foreach (var rend in newPlayerView.GetComponentsInChildren<Renderer>())
        {
            rend.materials = Enumerable.Repeat(ghostMaterial, rend.materials.Length).ToArray();
        }
    }

    private float GetCurrentMaxSpeed()
    {
        var moveDir = GameInput.moveDirection;
        moveDir.y = 0f;
        moveDir.Normalize();
        float num2 = 0f;
        var motor = (UnderwaterMotor)Player.main.playerController.underWaterController;
        if (moveDir.z > 0f)
        {
            num2 = motor.forwardMaxSpeed;
        }
        else if (moveDir.z < 0f)
        {
            num2 = motor.backwardMaxSpeed;
        }
        if (moveDir.x != 0f)
        {
            num2 = Mathf.Max(num2, motor.strafeMaxSpeed);
        }
        num2 = Mathf.Max(num2, motor.verticalMaxSpeed);
        float num3 = num2;
        num2 = motor.AlterMaxSpeed(num3);
        num2 *= motor.playerController.player.mesmerizedSpeedMultiplier;
        num2 *= motor.debugSpeedMult;
        return num2;
    }

    public override GameInput.Button GetUseButton() => GameInput.Button.Sprint;
    
    public override void OnEquipped()
    {
        RetrieveIndicatorReference();
        speedData = new SpeedData();
        currentBlinkResource = maxBlinkDuration;
    }

    private void OnDestroy()
    {
        Destroy(ghostMaterial);
        Inventory.main.equipment.onEquip -= RefreshIonManager;
    }

    private struct SpeedData
    {
        public float forwardsSpeed;
        public float backwardsSpeed;
        public float strafeSpeed;
        public float verticalSpeed;
        public float waterAcceleration;
        
        public void AssignToMotor(PlayerMotor motor)
        {
            motor.forwardMaxSpeed = forwardsSpeed;
            motor.backwardMaxSpeed = backwardsSpeed;
            motor.strafeMaxSpeed = strafeSpeed;
            motor.verticalMaxSpeed = verticalSpeed;
            motor.waterAcceleration = waterAcceleration;
        }

        public void CopyFromController(PlayerController playerController)
        {
            forwardsSpeed = playerController.swimForwardMaxSpeed;
            backwardsSpeed = playerController.swimBackwardMaxSpeed;
            strafeSpeed = playerController.swimStrafeMaxSpeed;
            verticalSpeed = playerController.swimVerticalMaxSpeed;
            waterAcceleration = playerController.swimWaterAcceleration;
        }

        public void Multiply(float factor)
        {
            forwardsSpeed *= factor;
            backwardsSpeed *= factor;
            strafeSpeed *= factor;
            verticalSpeed *= factor;
            waterAcceleration *= factor;
        }
    }
}