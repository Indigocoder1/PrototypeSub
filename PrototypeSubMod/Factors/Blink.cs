using UnityEngine;
using UWE;

namespace PrototypeSubMod.Factors;

public class Blink : Factor
{
    private const float SPEED_MULTIPLIER = 10f;
    private const float TIME_SCALE_SLOW = 0.5f;

    private PlayerController controller;
    private SpeedData originalSpeedData;
    
    public Blink()
    {
        duration = 1f;
        cooldown = 3f;
    }
    
    public override void Use()
    {
        if (Player.main.precursorOutOfWater || Player.main.transform.position.y > 0) return;
        
        controller = Player.main.playerController;

        if (controller == null)
        {
            Plugin.Logger.LogError("Failed to get Motor from Player for the BlinkFactor!");
            return;
        }
        
        Time.timeScale = TIME_SCALE_SLOW;
        originalSpeedData = new SpeedData(controller);
        (originalSpeedData * (SPEED_MULTIPLIER / TIME_SCALE_SLOW)).AssignToController(controller);
        controller.SetMotorMode(Player.MotorMode.Dive);
        
        ErrorMessage.AddDebug("Blink factor activated");
        CoroutineHost.StartCoroutine(WaitDuration());
    }

    public override GameInput.Button GetUseButton() => GameInput.Button.Sprint;

    public override void Disable()
    {
        Time.timeScale = 1f;
        originalSpeedData.AssignToController(controller);
        controller.SetMotorMode(Player.main.motorMode);
    }

    private struct SpeedData
    {
        public float forwardsSpeed;
        public float backwardsSpeed;
        public float strafeSpeed;
        public float verticalSpeed;
        public float waterAcceleration;

        public float seaglideForwardsSpeed;
        public float seaglideBackwardsSpeed;
        public float seaglideStrafeSpeed;
        public float seaglideVerticalSpeed;
        public float seaglideWaterAcceleration;

        public SpeedData(PlayerController playerController)
        {
            forwardsSpeed = playerController.swimForwardMaxSpeed;
            backwardsSpeed = playerController.swimBackwardMaxSpeed;
            strafeSpeed = playerController.swimStrafeMaxSpeed;
            verticalSpeed = playerController.swimVerticalMaxSpeed;
            waterAcceleration = playerController.swimWaterAcceleration;
            
            seaglideForwardsSpeed = playerController.seaglideForwardMaxSpeed;
            seaglideBackwardsSpeed = playerController.seaglideBackwardMaxSpeed;
            seaglideStrafeSpeed = playerController.seaglideStrafeMaxSpeed;
            seaglideVerticalSpeed = playerController.seaglideVerticalMaxSpeed;
            seaglideWaterAcceleration = playerController.seaglideWaterAcceleration;
        }

        public SpeedData(float forwardsSpeed, float backwardsSpeed, float strafeSpeed, float verticalSpeed, float waterAcceleration, float seaglideForwardsSpeed, float seaglideBackwardsSpeed, float seaglideStrafeSpeed, float seaglideVerticalSpeed, float seaglideWaterAcceleration)
        {
            this.forwardsSpeed = forwardsSpeed;
            this.backwardsSpeed = backwardsSpeed;
            this.strafeSpeed = strafeSpeed;
            this.verticalSpeed = verticalSpeed;
            this.waterAcceleration = waterAcceleration;
            this.seaglideForwardsSpeed = seaglideForwardsSpeed;
            this.seaglideBackwardsSpeed = seaglideBackwardsSpeed;
            this.seaglideStrafeSpeed = seaglideStrafeSpeed;
            this.seaglideVerticalSpeed = seaglideVerticalSpeed;
            this.seaglideWaterAcceleration = seaglideWaterAcceleration;
        }

        public SpeedData(SpeedData copyFrom)
        {
            forwardsSpeed = copyFrom.forwardsSpeed;
            backwardsSpeed = copyFrom.backwardsSpeed;
            strafeSpeed = copyFrom.strafeSpeed;
            verticalSpeed = copyFrom.verticalSpeed;
            waterAcceleration = copyFrom.waterAcceleration;
            seaglideForwardsSpeed = copyFrom.seaglideForwardsSpeed;
            seaglideBackwardsSpeed = copyFrom.seaglideBackwardsSpeed;
            seaglideStrafeSpeed = copyFrom.seaglideStrafeSpeed;
            seaglideVerticalSpeed = copyFrom.seaglideVerticalSpeed;
            seaglideWaterAcceleration = copyFrom.seaglideWaterAcceleration;
        }

        public void AssignToController(PlayerController playerController)
        {
            playerController.swimForwardMaxSpeed = forwardsSpeed;
            playerController.swimBackwardMaxSpeed = backwardsSpeed;
            playerController.swimStrafeMaxSpeed = strafeSpeed;
            playerController.swimVerticalMaxSpeed = verticalSpeed;
            playerController.swimWaterAcceleration = waterAcceleration;

            playerController.seaglideForwardMaxSpeed = seaglideForwardsSpeed;
            playerController.seaglideBackwardMaxSpeed = seaglideBackwardsSpeed;
            playerController.seaglideStrafeMaxSpeed = seaglideStrafeSpeed;
            playerController.seaglideVerticalMaxSpeed = seaglideVerticalSpeed;
            playerController.seaglideWaterAcceleration = seaglideWaterAcceleration;
        }

        public static SpeedData operator *(SpeedData data, float value)
        {
            return new SpeedData(data.forwardsSpeed * value, data.backwardsSpeed * value, data.strafeSpeed * value,
                data.verticalSpeed * value, data.waterAcceleration * value, 
                data.seaglideForwardsSpeed * value,
                data.seaglideBackwardsSpeed * value, data.seaglideStrafeSpeed * value,
                data.seaglideVerticalSpeed * value, data.seaglideWaterAcceleration * value);
        }
        
        public static SpeedData operator /(SpeedData data, float value)
        {
            return new SpeedData(data.forwardsSpeed / value, data.backwardsSpeed / value, data.strafeSpeed / value,
                data.verticalSpeed / value, data.waterAcceleration / value, 
                data.seaglideForwardsSpeed / value,
                data.seaglideBackwardsSpeed / value, data.seaglideStrafeSpeed / value,
                data.seaglideVerticalSpeed / value, data.seaglideWaterAcceleration / value);
        }
    }
}