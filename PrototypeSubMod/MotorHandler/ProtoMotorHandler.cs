using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.MotorHandler;

internal class ProtoMotorHandler : MonoBehaviour
{
    [SerializeField] private CyclopsMotorMode motorMode;

    private bool allowedToMove;
    private float[] originalMotorSpeeds;
    private float[] originalPowerValues;
    private float originalTurningTorque;

    private readonly Dictionary<Component, float> speedMultipliers = new();
    private readonly Dictionary<Component, float> speedBonuses = new();
    private readonly Dictionary<Component, float> powerEfficiencyMultipliers = new();
    private readonly Dictionary<Component, float> overrideNoiseValues = new();
    private readonly Dictionary<Component, float> turningTorqueMultipliers = new();

    private void Start()
    {
        originalTurningTorque = motorMode.subController.BaseTurningTorque;
        
        originalMotorSpeeds = new float[motorMode.motorModeSpeeds.Length];
        Array.Copy(motorMode.motorModeSpeeds, originalMotorSpeeds, motorMode.motorModeSpeeds.Length);
        originalPowerValues = new float[motorMode.motorModePowerConsumption.Length];
        Array.Copy(motorMode.motorModePowerConsumption, originalPowerValues, motorMode.motorModePowerConsumption.Length);
    }

    public void SetAllowedToMove(bool allowedToMove)
    {
        this.allowedToMove = allowedToMove;
    }

    public void AddSpeedMultiplier(ValueRegistrar speedMultiplier)
    {
        if (speedMultipliers.ContainsKey(speedMultiplier.component))
        {
            speedMultipliers[speedMultiplier.component] = speedMultiplier.value;
            return;
        }

        speedMultipliers.Add(speedMultiplier.component, speedMultiplier.value);
    }
    
    public bool RemoveSpeedMultiplier(Component component)
    {
        if (!speedMultipliers.ContainsKey(component)) return false;

        speedMultipliers.Remove(component);
        return true;
    }
    
    public void AddTurningTorqueMultiplier(ValueRegistrar torqueMultiplier)
    {
        if (turningTorqueMultipliers.ContainsKey(torqueMultiplier.component))
        {
            turningTorqueMultipliers[torqueMultiplier.component] = torqueMultiplier.value;
            return;
        }

        turningTorqueMultipliers.Add(torqueMultiplier.component, torqueMultiplier.value);
    }
    
    public bool RemoveTurningTorqueMultiplier(Component component)
    {
        return turningTorqueMultipliers.Remove(component);
    }

    /// <summary>
    /// Adds the given speed parameter to the existing speed multiplier
    /// </summary>
    /// <param name="extraSpeed"></param>
    public void AddSpeedBonus(ValueRegistrar multiplierBonus)
    {
        if (speedBonuses.ContainsKey(multiplierBonus.component))
        {
            speedBonuses[multiplierBonus.component] = multiplierBonus.value;
            return;
        }

        speedBonuses.Add(multiplierBonus.component, multiplierBonus.value);
    }

    public bool RemoveSpeedBonus(Component component)
    {
        if (!speedBonuses.ContainsKey(component)) return false;

        speedBonuses.Remove(component);
        return true;
    }

    public void RemoveAllSpeedMultipliers()
    {
        speedMultipliers.Clear();
    }

    public void RemoveAllSpeedBonuses()
    {
        speedBonuses.Clear();
    }

    public float GetNormalizedSpeed()
    {
        if (GetMaxSpeed() == 0) return 0;
        
        return Mathf.Clamp01(motorMode.subRoot.rigidbody.velocity.magnitude / GetMaxSpeed());
    }

    /// <summary>
    /// Set this to -1 if you don't want to change it
    /// </summary>
    /// <param name="noiseValue"></param>
    public void AddOverrideNoiseValue(ValueRegistrar overrideValue)
    {
        if (overrideNoiseValues.ContainsKey(overrideValue.component))
        {
            overrideNoiseValues[overrideValue.component] = overrideValue.value;
            return;
        }

        overrideNoiseValues.Add(overrideValue.component, overrideValue.value);
    }

    public bool RemoveOverrideNoiseValue(Component component)
    {
        if (!overrideNoiseValues.ContainsKey(component)) return false;

        overrideNoiseValues.Remove(component);
        return true;
    }

    public void RemoveAllNoiseOverrides()
    {
        overrideNoiseValues.Clear();
    }

    /// <summary>
    /// Controls the power efficiency of the engine. Higher number = more efficient. 1 = default
    /// </summary>
    /// <param name="multiplier"></param>
    /// <returns></returns>
    public void AddPowerEfficiencyMultiplier(ValueRegistrar efficiencyMultiplier)
    {
        if (powerEfficiencyMultipliers.ContainsKey(efficiencyMultiplier.component))
        {
            powerEfficiencyMultipliers[efficiencyMultiplier.component] = efficiencyMultiplier.value;
            return;
        }

        powerEfficiencyMultipliers.Add(efficiencyMultiplier.component, efficiencyMultiplier.value);
    }

    public bool RemovePowerEfficiencyMultiplier(Component component)
    {
        if (!powerEfficiencyMultipliers.ContainsKey(component)) return false;

        powerEfficiencyMultipliers.Remove(component);
        return true;
    }

    public void RemoveAllPowerMultipliers()
    {
        powerEfficiencyMultipliers.Clear();
    }

    public float GetEfficiencyMultiplier()
    {
        float total = 1;
        foreach (var registrar in powerEfficiencyMultipliers)
        {
            total *= registrar.Value;
        }

        return total;
    }

    public float GetOverrideNoiseValue()
    {
        if (overrideNoiseValues.Count == 0) return -1;

        float total = 0;
        foreach (var item in overrideNoiseValues)
        {
            total += item.Value;
        }

        return total;
    }

    private void OnUpdate()
    {
        if (originalMotorSpeeds == null) return;
        
        if (!allowedToMove)
        {
            motorMode.motorModeSpeeds = new float[3];
            motorMode.ChangeCyclopsMotorMode(motorMode.cyclopsMotorMode);
            motorMode.subController.BaseTurningTorque = 0;
            return;
        }

        float speedMultiplier = 1f;
        foreach (var item in speedMultipliers)
        {
            speedMultiplier *= item.Value;
        }

        float speedBonus = 0f;
        foreach (var item in speedBonuses)
        {
            speedBonus += item.Value;
        }

        float[] newSpeeds = new float[originalMotorSpeeds.Length];
        for (int i = 0; i < newSpeeds.Length; i++)
        {
            newSpeeds[i] = originalMotorSpeeds[i] * speedMultiplier + speedBonus;
        }
        
        float torqueMultiplier = 1f;
        foreach (var multiplier in turningTorqueMultipliers)
        {
            torqueMultiplier *= multiplier.Value;
        }
        
        motorMode.motorModeSpeeds = newSpeeds;
        motorMode.ChangeCyclopsMotorMode(motorMode.cyclopsMotorMode);
        motorMode.subController.BaseTurningTorque = originalTurningTorque * torqueMultiplier;
    }

    public bool GetAllowedToMove()
    {
        return allowedToMove;
    }

    public float GetMaxSpeed()
    {
        return motorMode.motorModeSpeeds[1];
    }

    private void OnEnable()
    {
        ManagedUpdate.Subscribe(ManagedUpdate.Queue.Update, OnUpdate);
    }

    private void OnDisable()
    {
        ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.Update, OnUpdate);
    }

    public struct ValueRegistrar
    {
        public Component component;
        public float value;

        /// <summary>
        /// Creates a new value registrar
        /// </summary>
        /// <param name="component">The component registering the value</param>
        /// <param name="value">The value to register</param>
        public ValueRegistrar(Component component, float value)
        {
            this.component = component;
            this.value = value;
        }
    }
}
