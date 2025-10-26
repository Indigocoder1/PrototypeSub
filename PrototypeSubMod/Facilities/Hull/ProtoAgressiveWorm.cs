using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull;

public class ProtoAggressiveWorm : Creature
{
    public event Action onDespawn; 
    
    [SerializeField] private ProtoWormSpineManager spineManager;
    [SerializeField] private Color passiveEmissionColor;
    [SerializeField] private Color aggressiveEmissionColor;
    [SerializeField] private GameObject headObject;
    [SerializeField] private float secondsInVoidForAggression;

    private Renderer[] headRenderers;
    private List<Renderer>[] segmentRenderers;
    private VFXElectricArcs[] electricArcs;
    private float secondsInVoid;
    private int segmentCount;
    private int numSegmentsAggressiveLastFrame;
    
    public override void Start()
    {
        base.Start();

        liveMixin.invincible = true;
        GetComponent<Rigidbody>().useGravity = false;
        StartCoroutine(RetrieveSegmentRends());
        headRenderers = headObject.GetComponentsInChildren<Renderer>();
        segmentCount = spineManager.GetSpineSegmentCount();
    }

    private IEnumerator RetrieveSegmentRends()
    {
        yield return new WaitUntil(() => spineManager.GetSpawned());
        yield return new WaitUntil(() => spineManager.GetChild(0).GetComponentInChildren<VFXElectricArcs>(true));
        
        var segmentCount = spineManager.GetSpineSegmentCount();
        segmentRenderers = new List<Renderer>[segmentCount];
        electricArcs = new VFXElectricArcs[segmentCount - 1];
        for (int i = 0; i < segmentCount; i++)
        {
            var child = spineManager.GetChild(i);
            segmentRenderers[i] = child.GetComponentsInChildren<Renderer>(true).ToList();
            if (i == segmentCount - 1) continue;

            electricArcs[i] = child.GetComponentInChildren<VFXElectricArcs>(true);
        }
    }

    private void Update()
    {
        var biomeString = Player.main.GetBiomeString();
        bool inVoid = biomeString is "void" or "";
        if (secondsInVoid < secondsInVoidForAggression && inVoid)
        {
            secondsInVoid += Time.deltaTime;
        }
        else if (secondsInVoid > 0 && !inVoid)
        {
            secondsInVoid -= Time.deltaTime;
        }
        
        var segmentsAggressive = (int)(secondsInVoid / secondsInVoidForAggression * segmentCount);

        if (segmentsAggressive != numSegmentsAggressiveLastFrame)
        {
            UpdateSegmentColors(segmentsAggressive);
        }
        
        numSegmentsAggressiveLastFrame = segmentsAggressive;
    }

    private void UpdateSegmentColors(int segmentsAggressive)
    {
        for (var i = 0; i < segmentCount; i++)
        {
            if (segmentRenderers == null) break;
            
            if (!spineManager.transform.GetChild(i).gameObject.activeSelf) continue;
            
            var isAggressive = i >= segmentCount - segmentsAggressive;
            foreach (var rend in segmentRenderers[i])
            {
                UpdateRendererEmissionColor(rend, isAggressive);
            }
            
            if (i == segmentCount - 1 || electricArcs == null) continue;
            UpdateArcColors(electricArcs[i], isAggressive);
        }

        foreach (var headRenderer in headRenderers)
        {
            UpdateRendererEmissionColor(headRenderer, segmentsAggressive == segmentCount);
        }
    }

    private void UpdateRendererEmissionColor(Renderer rend, bool aggressive)
    {
        var color = aggressive ? aggressiveEmissionColor : passiveEmissionColor;
        var materials = rend.materials;
        foreach (var mat in materials)
        {
            mat.SetColor(ShaderPropertyID._GlowColor, color);
        }
        rend.materials = materials;
    }

    private void UpdateArcColors(VFXElectricArcs arcs, bool aggressive)
    {
        var color = aggressive ? aggressiveEmissionColor : passiveEmissionColor;
        foreach (var line in arcs.lines)
        {
            line.line.material.color = color;
        }
    }

    public void ResetAggression(float timeToBecomeAggressive)
    {
        secondsInVoid = 0;
        secondsInVoidForAggression = timeToBecomeAggressive;
    }

    public bool IsAggressive() => secondsInVoid >= secondsInVoidForAggression;

    public override void OnDestroy()
    {
        onDespawn?.Invoke();
    }
}