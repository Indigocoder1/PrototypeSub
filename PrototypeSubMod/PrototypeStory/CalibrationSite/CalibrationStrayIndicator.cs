using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationStrayIndicator : MonoBehaviour
{
    private static readonly int TooFarFromDistance = Animator.StringToHash("StrayingFromLine");
    private static readonly int NormalizedDistFromLine = Animator.StringToHash("NormalizedDistFromLine");

    [SerializeField] private CalibrationRunManager runManager;
    [SerializeField] private Animator strayIndicator;
    [Range(0, 1)]
    [SerializeField] private float indicatorActivationThreshold;

    private void FixedUpdate()
    {
        var distFromCenter = runManager.GetNormalizedDistFromCenter();
        var straying = distFromCenter > indicatorActivationThreshold;
        strayIndicator.SetBool(TooFarFromDistance, straying);
        var strayAmount = Mathf.Clamp01((Mathf.Clamp01(distFromCenter) - indicatorActivationThreshold) / (1  - indicatorActivationThreshold));
        strayIndicator.SetFloat(NormalizedDistFromLine, strayAmount);
    }
}