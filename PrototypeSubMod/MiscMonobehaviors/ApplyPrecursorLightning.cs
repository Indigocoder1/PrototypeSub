using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class ApplyPrecursorLightning : MonoBehaviour
{
    [SerializeField] private Transform[] linkObjects;
    [SerializeField] private bool clearVFXControllers;
    [SerializeField] private float lineWidth;
    
    private readonly List<VFXElectricArcs> electricArcs = new();
    private GameObject electricArcPrefab;
    
    private void Awake()
    {
        if (linkObjects.Length < 2)
        {
            Plugin.Logger.LogError($"{gameObject} does not at least 2 link objects to link with lightning");
            return;
        }
        
        UWE.CoroutineHost.StartCoroutine(RetrievePrefab());
        UWE.CoroutineHost.StartCoroutine(InitializeArcs());
    }
    
    private IEnumerator RetrievePrefab()
    {
        if (electricArcPrefab) yield break;

        var task = UWE.PrefabDatabase.GetPrefabAsync("e8143977-448e-4202-b780-83485fa5f31a");
        yield return task;

        if (!task.TryGetPrefab(out var antechamberPrefab))
            throw new System.Exception("Error loading antechamber prefab");

        var vfxController = antechamberPrefab.GetComponent<VFXController>();
        electricArcPrefab = vfxController.emitters[0].fx;
    }
    
    private IEnumerator InitializeArcs()
    {
        yield return new WaitUntil(() => electricArcPrefab);

        SpawnArcs();
    }
    
    private void SpawnArcs()
    {
        for (int i = 0; i < linkObjects.Length - 1; i++)
        {
            var child1 = linkObjects[i];
            var child2 = linkObjects[i + 1];
            var instance = Instantiate(electricArcPrefab, child1.position, Quaternion.identity, child1);
            var electricArc = instance.GetComponent<VFXElectricArcs>();
            electricArc.target = child2;
            electricArc.Play();

            if (!clearVFXControllers)
            {
                electricArcs.Add(electricArc);
            }
            else
            {
                UWE.CoroutineHost.StartCoroutine(InitArcs(electricArc));
            }
        }
    }

    private IEnumerator InitArcs(VFXElectricArcs electricArc)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        foreach (var line in electricArc.lines)
        {
            line.line.useWorldSpace = false;
            line.line.startWidth = lineWidth;
            line.line.endWidth = lineWidth;

            Vector3 endPoint = electricArc.target.position - electricArc.transform.parent.position;
            for (int j = 0; j < line.segments; j++)
            {
                line.line.SetPosition(j, Vector3.Lerp(Vector3.zero, endPoint, (float)j / (line.segments - 1)));
            }

            line.enabled = false;
            line.line.enabled = true;
            line.line.material.mainTextureOffset = Vector2.zero;
            
            var vector = line.line.material.GetTextureOffset(ShaderPropertyID._DeformMap);
            line.line.material.SetTextureOffset(ShaderPropertyID._DeformMap, vector);
        }
    }
    
    private void OnEnable()
    {
        foreach (var arc in electricArcs)
        {
            arc.Play();
        }
    }
    
    private void OnDisable()
    {
        foreach (var arc in electricArcs)
        {
            arc.Stop();
        }
    }
}