using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class DefaultMaterialModifier : MaterialModifier
{
    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        
    }

    public override bool BlockShaderConversion(Material material, Renderer renderer, MaterialUtils.MaterialType materialType)
    {
        return renderer.gameObject.TryGetComponent<DontApplySNShaders>(out _);
    }
}