using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrototypeSubMod.Patches;
using PrototypeSubMod.Prefabs;
using SubLibrary.Handlers;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

namespace PrototypeSubMod.Factors;

public class Blink : Factor
{
    private float speedMultiplier = 7.5f;
    private float timeScaleSlow = 0.25f;
    
    private float maxBlinkDuration = 3f;
    private float blinkRechargeRate = 3 / 5f;

    private float timeBetweenGhostFrames = 0.25f;

    private float chromaticAbberationVal = 3f;
    private float depthOfFieldVal = 0.1f;
    private float fovMultiplier = 1.2f;

    private ChromaticAberrationModel.Settings originalChromaticSettings;
    private DepthOfFieldModel.Settings originalDepthOfFieldSettings;
    private bool wasChromaticActive;
    private List<GameObject> ghostFrames = new();
    private Material ghostMaterial;
    private Image chargeIndicator;
    private PlayerController controller;
    private SpeedData speedData;
    private float currentBlinkResource;
    private float timeNextGhostFrame;
    private bool fullyDepleted;

    private void Awake()
    {
        var ghostBorder =
            CyclopsReferenceHandler.CyclopsReference.transform.Find(
                "HolographicDisplay/HolographicDisplayVisuals/CyclopsMini_Mid/border");
        ghostMaterial = new(ghostBorder.GetComponent<Renderer>().material);
        ghostMaterial.color = new Color(0.443f, 1, 0.443f);
    }

    public override void Use()
    {
        if (fullyDepleted) return;
        
        if (Player.main.precursorOutOfWater || Player.main.transform.position.y > 0) return;
        
        if (Player.main.isPiloting) return;
        
        if (Player.main.currentSub != null) return;
        
        controller = Player.main.playerController;

        if (controller == null)
        {
            Plugin.Logger.LogError("Failed to get Motor from Player for the BlinkFactor!");
            return;
        }

        base.Use();
        
        speedData.CopyFromController(controller);
        speedData.Multiply(speedMultiplier / timeScaleSlow);
        speedData.AssignToMotor(controller.underWaterController);
        var moveDir = MainCameraControl.main.transform.right * GameInput.moveDirection.normalized.x +
            MainCameraControl.main.transform.forward * GameInput.moveDirection.normalized.z +
            MainCameraControl.main.transform.up * GameInput.moveDirection.normalized.y;
        Player.main.rigidBody.velocity = moveDir * (controller.swimForwardMaxSpeed * (speedMultiplier / timeScaleSlow));
        
        UWE.CoroutineHost.StartCoroutine(SetTimescaleDelayed(timeScaleSlow));
        ErrorMessage.AddDebug("Blink factor activated");
        PlayerController_Patches.SetBlockMotorModeAssignment(true);
        
        SNCameraRoot.main.SetFov(MiscSettings.fieldOfView * fovMultiplier);
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
    }
    
    public override void StopUse()
    {
        base.StopUse();

        // Multiply by the inverse instead of dividing
        speedData.Multiply(timeScaleSlow / speedMultiplier);
        speedData.AssignToMotor(controller.underWaterController);
        UWE.CoroutineHost.StartCoroutine(SetModeDelayed());
        UWE.CoroutineHost.StartCoroutine(SetTimescaleDelayed(1));
        Player.main.rigidBody.velocity = Vector3.zero;
        PlayerController_Patches.SetBlockMotorModeAssignment(false);
        
        SNCameraRoot.main.SetFov(MiscSettings.fieldOfView * fovMultiplier);
        var postProcessing = SNCameraRoot.main.mainCam.GetComponent<PostProcessingBehaviour>();
        postProcessing.profile.chromaticAberration.enabled = wasChromaticActive;
        postProcessing.profile.chromaticAberration.settings = originalChromaticSettings;
        postProcessing.profile.depthOfField.settings = originalDepthOfFieldSettings;

        foreach (var ghost in ghostFrames)
        {
            Destroy(ghost);
        }

        ghostFrames.Clear();
    }

    private IEnumerator SetTimescaleDelayed(float timeScale)
    {
        // Wait until a FixedUpdate has ocurred to actually update the player velocity
        var timestepIncrements = (int)(Time.fixedUnscaledTime / Time.fixedUnscaledDeltaTime);
        while (timestepIncrements == (int)(Time.fixedUnscaledTime / Time.fixedUnscaledDeltaTime))
        {
            yield return null;
        }

        Time.timeScale = timeScale;
    }

    private IEnumerator SetModeDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        if (inUse) yield break;
        
        controller.SetMotorMode(Player.main.motorMode);
    }

    private void RetrieveIndicatorReference()
    {
        if (chargeIndicator != null) return;
        
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
        
        chargeIndicator = hudContent.Find("BlinkFactorCharge/Mask").GetComponent<Image>();
    }
    
    public override void UpdateFactor()
    {
        if (inUse && currentBlinkResource > 0)
        {
            currentBlinkResource -= Time.unscaledDeltaTime;
            if (currentBlinkResource <= 0)
            {
                fullyDepleted = true;
            }
        }
        else if (currentBlinkResource < maxBlinkDuration)
        {
            currentBlinkResource += Time.deltaTime * blinkRechargeRate;
        }
        else if (fullyDepleted)
        {
            fullyDepleted = false;
        }

        if (chargeIndicator != null)
        {
            chargeIndicator.fillAmount = currentBlinkResource / maxBlinkDuration;
        }

        if (Time.time > timeNextGhostFrame && inUse)
        {
            timeNextGhostFrame = Time.time + timeBetweenGhostFrames * timeScaleSlow;
            SpawnGhostFrame();
        }

        if (currentBlinkResource <= 0)
        {
            StopUse();
        }
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

    public override GameInput.Button GetUseButton() => GameInput.Button.Sprint;
    
    public override void OnEquipped()
    {
        RetrieveIndicatorReference();
        chargeIndicator.gameObject.SetActive(true);
        speedData = new SpeedData();
        currentBlinkResource = maxBlinkDuration;
    }
    
    public override void OnUnequipped()
    {
        chargeIndicator.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Destroy(ghostMaterial);
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