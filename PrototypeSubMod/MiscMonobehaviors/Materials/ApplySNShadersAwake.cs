using Nautilus.Utility;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class ApplySNShadersAwake : MonoBehaviour
{
    private void Awake()
    {
        MaterialUtils.ApplySNShaders(gameObject, modifiers: new DefaultMaterialModifier());
    }
}