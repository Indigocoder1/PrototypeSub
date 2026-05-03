using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class EmissionAreaManager : MonoBehaviour
{
    private static readonly int UVTarget = Shader.PropertyToID("_UVTarget");
    private static readonly int EmissiveStrength = Shader.PropertyToID("_EmissiveStrength");
    
    [SerializeField] private GameObject objectRoot;
    [SerializeField] private bool collectRenderersAtStart;

    private Renderer[] renderers;

    private void Start()
    {
        if (!collectRenderersAtStart) return;

        renderers = objectRoot.GetComponentsInChildren<Renderer>();
    }

    public void UpdateRenderers()
    {
        renderers = objectRoot.GetComponentsInChildren<Renderer>();
    }

    public void SetValues(float progress, float intensity)
    {
        foreach (var rend in renderers)
        {
            foreach (var material in rend.materials)
            {
                material.SetFloat(UVTarget, progress);
                material.SetFloat(EmissiveStrength, intensity);
            }
        }
    }
}