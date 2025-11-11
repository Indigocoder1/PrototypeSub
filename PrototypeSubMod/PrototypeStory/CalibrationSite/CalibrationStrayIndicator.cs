using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationStrayIndicator : MonoBehaviour
{
    private static readonly int StrayingFromLine = Animator.StringToHash("StrayingFromLine");
    private static readonly int NormalizedDistFromLine = Animator.StringToHash("NormalizedDistFromLine");

    [SerializeField] private CalibrationRunManager runManager;
    [SerializeField] private Animator strayIndicator;
    [Range(0, 1)]
    [SerializeField] private float indicatorActivationThreshold;

    private void FixedUpdate()
    {
        var distFromLine = runManager.GetNormalizedDistFromLine();
        bool straying = distFromLine > indicatorActivationThreshold;
        strayIndicator.SetBool(StrayingFromLine, straying);
        float strayAmount = Mathf.Clamp01((Mathf.Clamp01(distFromLine) - indicatorActivationThreshold) / (1  - indicatorActivationThreshold));
        strayIndicator.SetFloat(NormalizedDistFromLine, strayAmount);
    }
}