using System;
using System.Collections;
using Nautilus.Utility;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class VFXConstructingManager : MonoBehaviour
{
    [SerializeField] private VFXConstructing vfxConstructing;
    [SerializeField] private float timeToConstruct;

    private Coroutine deconstructionCoroutine;

    public void ActivateConstruction()
    {
        vfxConstructing.RevertMaterials();
        vfxConstructing.EndConstruct();

        if (deconstructionCoroutine != null)
        {
            UWE.CoroutineHost.StopCoroutine(deconstructionCoroutine);
        }

        vfxConstructing.isDone = false;
        vfxConstructing.timeToConstruct = timeToConstruct;
        vfxConstructing.informGameObject = gameObject;
        vfxConstructing.enabled = true;

        vfxConstructing.Construct();
    }

    public void ActivateDeconstruction()
    {
        vfxConstructing.RevertMaterials();
        vfxConstructing.EndConstruct();
        
        if (deconstructionCoroutine != null)
        {
            UWE.CoroutineHost.StopCoroutine(deconstructionCoroutine);
        }

        vfxConstructing.isDone = false;
        deconstructionCoroutine = UWE.CoroutineHost.StartCoroutine(DeconstructAsync());
    }

    private IEnumerator DeconstructAsync()
    {
        vfxConstructing.ghostOverlay = vfxConstructing.gameObject.EnsureComponent<VFXOverlayMaterial>();
        vfxConstructing.Construct();
        
        Shader.SetGlobalFloat(ShaderPropertyID._SubConstructProgress, 1);

        yield return new WaitForSeconds(0.15f);
        
        var timer = timeToConstruct;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            Shader.SetGlobalFloat(ShaderPropertyID._SubConstructProgress, timer / timeToConstruct);
            yield return null;
        }

        vfxConstructing.EndConstruct();
        vfxConstructing.RevertMaterials();
    }
}