using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrototypeSubMod.Utility;

// This class is pretty much a copy of the PrecursorMaterialModifier in RotA, which coincidentally is more feature-rich. Make sure to check it out!
internal class ProtoMaterialModifier : MaterialModifier
{
    private float _specInt;
    private float _fresnelStrength;
    private bool applyPrecursorChanges;
    
    private static readonly int _specIntID = Shader.PropertyToID("_SpecInt");

    private static readonly int _fresnelID = Shader.PropertyToID("_Fresnel");

    private static Color precursorSpecularGreen = new Color(0.25f, 0.54f, 0.41f);

    private Dictionary<(Renderer, int), MaterialData> materialDatas = new();

    public ProtoMaterialModifier(float specInt, float fresnelStrength = 0.4f, bool applyPrecursorChanges = true)
    {
        _specInt = specInt;
        _fresnelStrength = fresnelStrength;
        this.applyPrecursorChanges = applyPrecursorChanges;
    }

    public override bool BlockShaderConversion(Material material, Renderer renderer, MaterialUtils.MaterialType materialType)
    {
        if (renderer is ParticleSystemRenderer) return true;

        if (!renderer.TryGetComponent<DontApplySNShaders>(out var dontApply)) return false;

        return dontApply.blacklistedMaterials.Count == 0 || dontApply.blacklistedMaterials.Any(m => m.shader == material.shader);
    }

    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        if (renderer.TryGetComponent<DontApplyProtoMaterial>(out _)) return;

        string matName = material.name.ToLower();
        if (matName.Contains("transparent") && applyPrecursorChanges)
        {
            var materials = renderer.materials;
            materials[materialIndex] = MaterialUtils.PrecursorGlassMaterial;
            renderer.materials = materials;
        }

        if (materialDatas.TryGetValue((renderer, materialIndex), out var materialData))
        {
            if (material.HasProperty("_GlowColor"))
            {
                material.SetColor("_GlowColor", materialData.emissionColor);
            }
            material.SetFloat("_GlowStrength", materialData.emissionIntensity);
            material.SetFloat("_GlowStrengthNight", materialData.emissionIntensity);
        }

        if (!applyPrecursorChanges) return;
        
        material.SetColor(ShaderPropertyID._SpecColor, precursorSpecularGreen);
        material.SetFloat(_specIntID, _specInt);
        material.SetFloat(_fresnelID, _fresnelStrength);
    }

    public void OnPreShaderConversion(Material material, Renderer renderer)
    {
        if (renderer.TryGetComponent<DontApplyProtoMaterial>(out _)) return;

        if (material.HasProperty("_EmissionColor"))
        {
            var emissionColor = material.GetColor("_EmissionColor");
            float emissionIntensity = Mathf.Max(emissionColor.r, emissionColor.g, emissionColor.b);

            int index = Array.IndexOf(renderer.materials, material);
            materialDatas.Add((renderer, index), new MaterialData(emissionIntensity, emissionColor / emissionIntensity));
        }
    }

    private struct MaterialData
    {
        public float emissionIntensity;
        public Color emissionColor;

        public MaterialData(float emissionIntensity, Color emissionColor)
        {
            this.emissionIntensity = emissionIntensity;
            this.emissionColor = emissionColor;
        }
    }
}
