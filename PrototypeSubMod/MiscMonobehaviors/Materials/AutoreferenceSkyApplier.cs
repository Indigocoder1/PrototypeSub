using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

internal class AutoreferenceSkyApplier : SkyApplier
{
    [SerializeField] private bool overrideSky;
    [SerializeField] private Vector3 overrideSkyPos;
    
    private new void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        if (overrideSky)
        {
            anchorSky = Skies.Custom;
            environmentSky = WaterBiomeManager.main.GetBiomeEnvironment(overrideSkyPos);
        }
        
        UWE.CoroutineHost.StartCoroutine(ApplySkyboxDelayed());
    }

    private IEnumerator ApplySkyboxDelayed()
    {
        yield return new WaitForSeconds(0.5f);

        if (this == null) yield break;
        
        var environment = GetEnvironment(gameObject, anchorSky);
        GetAndApplySkybox(environment);
    }

    private new void OnEnable()
    {
        UWE.CoroutineHost.StartCoroutine(ApplySkyboxDelayed());
        if (SkyApplierUpdater.main && !SkyApplierUpdater.main.skyAppliers.Contains(this))
        {
            SkyApplierUpdater.main.Add(this);
        }
    }
}
