using System;
using UnityEngine;

namespace PrototypeSubMod.DestructionEvent;

public class DarkEnergyRadiation : MonoBehaviour
{
    [SerializeField] private RadiatePlayerInRange radiatePlayerInRange;
    [SerializeField] private Color radiationColor;
    
    private RadiationsScreenFX radiationsScreenFX;
    private uGUI_RadiationWarning radiationWarning;
    private Color originalRadiationColor;
    private bool wasOutOfRange;

    private void Start()
    {
        radiationsScreenFX = Camera.main.GetComponent<RadiationsScreenFX>();

        radiationWarning = uGUI.main.transform.Find("ScreenCanvas/HUD/Content/RadiationWarning")
            .GetComponent<uGUI_RadiationWarning>();
    }

    private void Update()
    {
        var distance = Vector3.Distance(transform.position, Player.main.transform.position);
        var outOfRange = distance > radiatePlayerInRange.radiateRadius || !radiatePlayerInRange.enabled;

        if (outOfRange == wasOutOfRange) return;
        wasOutOfRange = outOfRange;
            
        // Just entered radiation range
        if (!outOfRange)
        {
            originalRadiationColor = radiationsScreenFX.color;
        }

        UpdateRadiationStatus();
    }

    public void UpdateRadiationStatus()
    {
        radiationsScreenFX.color = wasOutOfRange ? originalRadiationColor : radiationColor;
        radiationWarning.text.text = Language.main.Get(wasOutOfRange ? "RadiationDetected": "DarkMatterDetected");
    }
}