using UnityEngine;
using UWE;

namespace PrototypeSubMod.Factors;

public class Blink : Factor
{
    private const float SPEED_MULTIPLIER = 10f;

    private PlayerMotor motor;
    
    public Blink()
    {
        duration = 1f;
        cooldown = 10f;
    }
    
    public override void Use()
    {
        if (Player.main.precursorOutOfWater || Player.main.transform.position.y > 0) return;
        
        var waterMotor = Player.main.GetComponent<UnderwaterMotor>();
        
        motor = waterMotor.enabled ? waterMotor : Player.main.GetComponent<GroundMotor>();

        if (motor == null)
        {
            Plugin.Logger.LogError("Failed to get Motor from Player for the BlinkFactor!");
            return;
        }
        
        Time.timeScale = 0.5f;
        motor.forwardMaxSpeed *= SPEED_MULTIPLIER;
        motor.backwardMaxSpeed *= SPEED_MULTIPLIER;
        motor.strafeMaxSpeed *= SPEED_MULTIPLIER;
        motor.verticalMaxSpeed *= SPEED_MULTIPLIER;
        motor.waterAcceleration *= SPEED_MULTIPLIER * 2;
        motor.groundAcceleration *= SPEED_MULTIPLIER * 2;

        ErrorMessage.AddDebug("Blink factor activated");
        CoroutineHost.StartCoroutine(WaitDuration());
    }

    public override void Disable()
    {
        Time.timeScale = 1f;
        motor.forwardMaxSpeed /= SPEED_MULTIPLIER;
        motor.backwardMaxSpeed /= SPEED_MULTIPLIER;
        motor.strafeMaxSpeed /= SPEED_MULTIPLIER;
        motor.verticalMaxSpeed /= SPEED_MULTIPLIER;
        motor.waterAcceleration /= SPEED_MULTIPLIER * 2;
        motor.groundAcceleration /= SPEED_MULTIPLIER * 2;
        motor = null;
    }
}