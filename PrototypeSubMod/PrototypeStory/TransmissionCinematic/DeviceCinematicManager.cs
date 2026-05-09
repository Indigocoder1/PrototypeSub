using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic;

public class DeviceCinematicManager : MonoBehaviour
{
    private static readonly int EmissionLm = Shader.PropertyToID($"_EmissionLM");
    private static readonly int EmissionLmNight = Shader.PropertyToID("_EmissionLMNight");
    private static readonly int FiringPos = Animator.StringToHash("PrepFiringPos");
    private static readonly int Fire = Animator.StringToHash("Fire");
    private static readonly int ChargeUp = Animator.StringToHash("ChargeUp");

    [Header("Animators")]
    [SerializeField] private Animator deviceAnimator;
    [SerializeField] private Animator cinematicAnimator;
    
    [Header("Emission pulse")]
    [SerializeField] private Renderer ionCubesRenderer;
    [SerializeField] private int ionCrystalMatIndex;
    [SerializeField] private AnimationCurve emissionOverTime;
    
    public void PulseEmission(float duration = 1f, float intensityScalar = 1f)
    {
        StartCoroutine(PulseEmissionAsync(duration, intensityScalar));
    }

    private IEnumerator PulseEmissionAsync(float duration, float intensityScalar)
    {
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            var mat = ionCubesRenderer.materials[ionCrystalMatIndex];
            var emission = emissionOverTime.Evaluate(currentTime / duration) * intensityScalar;
            mat.SetFloat(EmissionLm, emission);
            mat.SetFloat(EmissionLmNight, emission);

            yield return null;
        }
    }

    public void PrepFiringPos()
    {
        cinematicAnimator.SetTrigger(FiringPos);
    }
    
    public void ChargeLaser()
    {
        deviceAnimator.SetTrigger(ChargeUp);
    }

    public void FireLaserAnim()
    {
        deviceAnimator.SetTrigger(Fire);
    }
}