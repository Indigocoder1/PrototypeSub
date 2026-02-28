using System;
using PrototypeSubMod.Puzzles.BearingPuzzle;
using PrototypeSubMod.UI;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationDirectionIndicator : MonoBehaviour
{
    [SerializeField] private CalibrationRunManager runManager;
    [SerializeField] private ProtoCompassManager compassManager;
    [SerializeField] private Color highlightedColor;

    private void Start()
    {
        CalibrationRunManager.OnPointReached += OnPointReached;
    }

    private void OnPointReached(int index)
    {
        var relativeAngles = runManager.GetRelativeAngles();
        if (index > relativeAngles.Length - 1)
        {
            compassManager.ClearHighlightedAngle();
        }
    }

    private void OnDestroy()
    {
        CalibrationRunManager.OnPointReached -= OnPointReached;
    }
}