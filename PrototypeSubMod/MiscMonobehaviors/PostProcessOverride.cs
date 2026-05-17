using UnityEngine;
using UnityEngine.PostProcessing;

namespace PrototypeSubMod.MiscMonobehaviors;

public class PostProcessOverride : MonoBehaviour
{
    [SerializeField] private CameraPostProcessApplier postProcessApplier;
    [SerializeField] private BloomModel bloomOverride;

    public void ApplyOverrides()
    {
        if (!postProcessApplier.GetComponentsAdded())
        {
            postProcessApplier.OnComponentsAdded += ApplyEffects;
            return;
        }

        ApplyEffects();
    }

    private void ApplyEffects()
    {
        var postProcessing = postProcessApplier.GetCamera().GetComponent<PostProcessingBehaviour>();
        postProcessing.profile.bloom = bloomOverride;
    }
}