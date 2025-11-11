using System.Collections;
using PrototypeSubMod.Patches;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Factors;

public class Blink : Factor
{
    private const float SPEED_MULTIPLIER = 7.5f;
    private const float TIME_SCALE_SLOW = 0.25f;
    
    private const float MAX_BLINK_DURATION = 3f;
    private const float BLINK_RECHARGE_RATE = MAX_BLINK_DURATION / 5f;

    private float startImpulse = 50f;

    private Image chargeIndicator;
    private PlayerController controller;
    private SpeedData speedData;
    private float currentBlinkResource = MAX_BLINK_DURATION;
    private bool fullyDepleted;
    
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
        
        Time.timeScale = TIME_SCALE_SLOW;
        speedData.CopyFromController(controller);
        speedData.Multiply(SPEED_MULTIPLIER / TIME_SCALE_SLOW);
        speedData.AssignToMotor(controller.underWaterController);
        Player.main.rigidBody.AddForce(GameInput.moveDirection.normalized * startImpulse, ForceMode.Impulse);
        
        ErrorMessage.AddDebug("Blink factor activated");
        PlayerController_Patches.SetBlockMotorModeAssignment(true);
    }
    
    public override void StopUse()
    {
        base.StopUse();

        // Multiply by the inverse instead of dividing
        speedData.Multiply(TIME_SCALE_SLOW / SPEED_MULTIPLIER);
        speedData.AssignToMotor(controller.underWaterController);
        UWE.CoroutineHost.StartCoroutine(ResetDrag(controller.underWaterController.swimDrag));
        UWE.CoroutineHost.StartCoroutine(EaseInTimescale());
        controller.underWaterController.swimDrag = 7f;
        PlayerController_Patches.SetBlockMotorModeAssignment(false);
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
        else if (currentBlinkResource < MAX_BLINK_DURATION)
        {
            currentBlinkResource += Time.deltaTime * BLINK_RECHARGE_RATE;
        }
        else if (fullyDepleted)
        {
            fullyDepleted = false;
        }

        if (chargeIndicator != null)
        {
            chargeIndicator.fillAmount = currentBlinkResource / MAX_BLINK_DURATION;
        }

        if (currentBlinkResource <= 0)
        {
            StopUse();
        }
    }

    public override GameInput.Button GetUseButton() => GameInput.Button.Sprint;

    private IEnumerator ResetDrag(float originalDrag)
    {
        yield return new WaitUntil(() => Player.main.rigidBody.velocity.magnitude < controller.swimForwardMaxSpeed);
        controller.underWaterController.swimDrag = originalDrag;
    }

    private IEnumerator EaseInTimescale()
    {
        float scale = Time.timeScale;
        while (scale < 1)
        {
            if (IngameMenu.main.gameObject.activeSelf) yield break;
            
            if (inUse) yield break;
            
            // Multiply by 2 to get done in 1/2 second
            scale += Time.unscaledDeltaTime * (1 - TIME_SCALE_SLOW) * 2f;
            Time.timeScale = scale;
            yield return new WaitForEndOfFrame();
        }

        Time.timeScale = 1;
    }
    
    public override void OnEquipped()
    {
        RetrieveIndicatorReference();
        chargeIndicator.gameObject.SetActive(true);
        speedData = new SpeedData();
    }
    
    public override void OnUnequipped()
    {
        chargeIndicator.gameObject.SetActive(false);
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