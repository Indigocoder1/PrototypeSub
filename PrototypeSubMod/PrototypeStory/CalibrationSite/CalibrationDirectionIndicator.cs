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
        runManager.onPointReached += OnPointReached;
        runManager.onCalibrationFailed += compassManager.ClearHighlightedAngle;
    }

    private void OnPointReached(int index)
    {
        var relativeAngles = runManager.GetRelativeAngles();
        if (index > relativeAngles.Length - 1)
        {
            compassManager.ClearHighlightedAngle();
            return;
        }

        compassManager.SetHighlightedAngle((360 + 90 - relativeAngles[index]) % 360, highlightedColor);
    }
}