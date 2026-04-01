using System;
using System.Collections;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class ApplyMaterialFromPrefab : MonoBehaviour, IMaterialModifier
{
    public event Action<GameObject> onEditMaterial;

    [SerializeField] private Renderer[] applyTo;
    [SerializeField] private string prefabClassID;
    [SerializeField] private string childPath;
    [SerializeField] private int copyIndex;
    [SerializeField] private int applyIndex;
    [SerializeField] private bool applyToSharedMaterials;

    private bool applied;
    
    private void Start()
    {
        CoroutineHost.StartCoroutine(ApplyMaterial());
    }

    public IEnumerator ApplyMaterial()
    {
        if (applied) yield break;
        
        applied = true;
        
        var task = PrefabDatabase.GetPrefabAsync(prefabClassID);
        yield return task;

        if (!task.TryGetPrefab(out var prefab)) throw new Exception($"Error loading prefab for {prefabClassID}");
        
        var childObj = string.IsNullOrEmpty(childPath) ? prefab : prefab.transform.Find(childPath).gameObject;
        if (!childObj)
        {
            throw new Exception($"Could not find child under {prefab} at path = {childPath}");
        }
        
        var rend = childObj.GetComponent<Renderer>();
        if (!rend)
        {
            throw new Exception($"Could not find a renderer on {childObj}");
        }

        foreach (var r in applyTo)
        {
            var mats = applyToSharedMaterials ? r.sharedMaterials : r.materials;
            mats[applyIndex] = rend.materials[copyIndex];

            if (applyToSharedMaterials)
            {
                r.sharedMaterials = mats;
            }
            else
            {
                r.materials = mats;
            }

            onEditMaterial?.Invoke(r.gameObject);
        }
    }
}