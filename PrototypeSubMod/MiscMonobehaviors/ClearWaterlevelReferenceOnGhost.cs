using System;
using PrototypeSubMod.Patches;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class ClearWaterlevelReferenceOnGhost : MonoBehaviour
{
    [SerializeField] private PrecursorMoonPoolTrigger trigger;

    private void OnValidate()
    {
        if (!trigger) TryGetComponent(out trigger);
    }

    private void OnEnable()
    {
        FreecamController_Patches.onExitFreecam += ForceOnTriggerExit;
    }

    private void OnDisable()
    {
        FreecamController_Patches.onExitFreecam -= ForceOnTriggerExit;
    }

    private void ForceOnTriggerExit()
    {
        if (Player.main.waterPlaneOverride != (IWaterPlane)trigger) return;
        
        trigger.OnTriggerExit(Player.mainCollider);
        Player.main.SetPrecursorOutOfWater(false);
    }
}