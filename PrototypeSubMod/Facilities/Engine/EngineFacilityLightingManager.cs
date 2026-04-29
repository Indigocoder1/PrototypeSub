using PrototypeSubMod.MiscMonobehaviors;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Engine;

public class EngineFacilityLightingManager : MonoBehaviour
{
    [SerializeField] private LightingController lightingController;
    [SerializeField] private SequencedLightEnabler sequencedLightEnabler;

    private void Start()
    {
        EngineFacilityRepairPoint.OnPointRepaired += UpdateLightingState;
        UpdateLightingState();
    }

    private void UpdateLightingState()
    {
        if (!Plugin.GlobalSaveData.EngineFacilityPointsRepaired) return;

        lightingController.LerpToState(2);
        sequencedLightEnabler.ActivateLightsSequentially();
    }

    private void OnDestroy()
    {
        EngineFacilityRepairPoint.OnPointRepaired -= UpdateLightingState;
    }
}