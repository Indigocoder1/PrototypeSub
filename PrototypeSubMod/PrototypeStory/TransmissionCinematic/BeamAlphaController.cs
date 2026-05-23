using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic;

public class BeamAlphaController : MonoBehaviour
{
    [SerializeField] private QepLaserSpawner laserSpawner;
    [SerializeField] private AnimationCurve alphaOverTime;

    public void PlayAlphaAnimation(float duration)
    {
        StartCoroutine(FadeAlphaAsync(duration));
    }

    private IEnumerator FadeAlphaAsync(float duration)
    {
        float timer = 0;
        var laserRenderer = laserSpawner.GetLaserObject().GetComponent<Renderer>();
        var targetMaterial = laserRenderer.materials[1];
        while (timer < duration)
        {
            var color = targetMaterial.color;
            color.a = alphaOverTime.Evaluate(timer / duration);
            targetMaterial.color = color;
            timer += Time.deltaTime;
            yield return null;
        }
    }
}