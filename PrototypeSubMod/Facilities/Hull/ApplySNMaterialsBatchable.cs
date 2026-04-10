using System;
using Nautilus.Utility;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull;

public class ApplySNMaterialsBatchable : MonoBehaviour
{
    [SerializeField] private GameObject applyTo;
    [SerializeField] private float shininess = 4;
    [SerializeField] private float specularIntensity = 1;
    [SerializeField] private float glowStrength = 1;

    private void OnValidate()
    {
        if (!applyTo) applyTo = gameObject;
    }

    private void Awake()
    {
        foreach (var rend in GetComponentsInChildren<Renderer>(true))
        {
            foreach (var material in rend.sharedMaterials)
            {
                if (material.shader == MaterialUtils.Shaders.MarmosetUBER) continue;
                
                MaterialUtils.ApplyUBERShader(material, shininess, specularIntensity, glowStrength, MaterialUtils.MaterialType.Opaque);
            }
        }
    }
}