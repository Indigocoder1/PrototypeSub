using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic;

public class DeviceCinematicManager : MonoBehaviour
{
    private static readonly int EmissionLm = Shader.PropertyToID($"_EmissionLM");
    private static readonly int EmissionLmNight = Shader.PropertyToID("_EmissionLMNight");
    
    [SerializeField] private Renderer ionCubesRenderer;
    [SerializeField] private int ionCrystalMatIndex;
    [SerializeField] private AnimationCurve emissionOverTime;
    
    public void PulseEmission(float duration = 1f)
    {
        StartCoroutine(PulseEmissionAsync(duration));
    }

    private IEnumerator PulseEmissionAsync(float duration)
    {
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            var mat = ionCubesRenderer.materials[ionCrystalMatIndex];
            var emission = emissionOverTime.Evaluate(currentTime / duration);
            mat.SetFloat(EmissionLm, emission);
            mat.SetFloat(EmissionLmNight, emission);

            yield return null;
        }
    }
}