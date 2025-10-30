using System;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using UnityEngine;

namespace PrototypeSubMod.Utility;

public class ApplyMaterialDatabase : MonoBehaviour, IMaterialModifier
{
    public event Action<GameObject> onEditMaterial;
    
    [SerializeField] private GameObject applyTo;

    private void OnValidate()
    {
        if (!applyTo) applyTo = gameObject;
    }

    private void Start()
    {
        if (applyTo == null) applyTo = gameObject;
        
        UWE.CoroutineHost.StartCoroutine(ProtoMatDatabase.ReplaceVanillaMats(applyTo, OnFinishedMaterialReplacement));
    }

    private void OnFinishedMaterialReplacement()
    {
        onEditMaterial?.Invoke(applyTo);
    }
}