using System;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class MaterialApplierCaller : MonoBehaviour
{
    [SerializeField] private ApplyMaterialFromPrefab[] materialAppliers;

    private void Start()
    {
        foreach (var applier in materialAppliers)
        {
            UWE.CoroutineHost.StartCoroutine(applier.ApplyMaterial());
        }
    }
}