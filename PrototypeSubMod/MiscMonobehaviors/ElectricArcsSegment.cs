using System;
using System.Collections;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace PrototypeSubMod.MiscMonobehaviors;

public class ElectricArcsSegment : MonoBehaviour, IMaterialModifier
{
    [SerializeField] private Transform arcTarget;
    [SerializeField] private bool setToTile;
    [SerializeField] private Vector2 textureScale = Vector2.one;
    [SerializeField] private float startWidth = 1f;
    [SerializeField] private float endWidth = 0.4f;
    
    public event Action<GameObject> onEditMaterial;

    private VFXElectricArcs electricArcs;
    
    private void Start()
    {
        UWE.CoroutineHost.StartCoroutine(RetrievePrefab());
    }
    
    private IEnumerator RetrievePrefab()
    {
        var task = UWE.PrefabDatabase.GetPrefabAsync("e8143977-448e-4202-b780-83485fa5f31a");
        yield return task;

        if (!task.TryGetPrefab(out var antechamberPrefab))
            throw new Exception("Error loading antechamber prefab");

        var vfxController = antechamberPrefab.GetComponent<VFXController>();
        var prefab = vfxController.emitters[0].fx;
        
        var instance = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        electricArcs = instance.GetComponent<VFXElectricArcs>();
        electricArcs.target = arcTarget;
        electricArcs.Play();

        yield return new WaitUntil(() => electricArcs.lines != null);

        foreach (var line in electricArcs.lines)
        {
            if (setToTile)
            {
                line.line.textureMode = LineTextureMode.Tile;
                line.line.material.SetTextureScale("_MainTex", textureScale);
            }

            line.line.startWidth = startWidth;
            line.line.endWidth = endWidth;
            
            onEditMaterial?.Invoke(line.line.gameObject);
        }
    }

    private void OnEnable()
    {
        if (electricArcs == null) return;
        
        electricArcs.Play();
    }
}