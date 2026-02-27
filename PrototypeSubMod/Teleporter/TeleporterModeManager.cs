using System;
using UnityEngine;

namespace PrototypeSubMod.Teleporter;

public class TeleporterModeManager : MonoBehaviour
{
    [SerializeField] private PrecursorTeleporter teleporter;
    [SerializeField] private TeleporterFXColorManager colorManager;
    [SerializeField] private GameObject interfloorCollider;
    [SerializeField] private GameObject normalCollider;
    [SerializeField] private Color interfloorColor;

    private void Start()
    {
        SetInterfloorMode();
        teleporter.ToggleDoor(true);
    }

    public void SetInterfloorMode()
    {
        colorManager.RemoveTempColor(this);
        colorManager.AddTempColor(this, new TeleporterFXColorManager.TempColor(interfloorColor, 10));
        normalCollider.SetActive(false);
        interfloorCollider.SetActive(true);
    }

    public void SetNormalMode()
    {
        colorManager.RemoveTempColor(this);
        colorManager.AddTempColor(this, new TeleporterFXColorManager.TempColor(TeleporterOverride.OverrideColor, 11));
        normalCollider.SetActive(true);
        interfloorCollider.SetActive(false);
    }

    public void OnConstructionFinished()
    {
        teleporter.ToggleDoor(true);
    }
}