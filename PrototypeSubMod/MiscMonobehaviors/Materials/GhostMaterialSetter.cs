using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

internal class GhostMaterialSetter : MonoBehaviour
{
    [SerializeField] private VFXConstructing vfxConstructing;
    [SerializeField] private Color ghostMatColor;

    // Called via Unity Event on CustomSubVFXConstructing
    public void OnConstructionStarted()
    {
        vfxConstructing.ghostMaterial = new Material(vfxConstructing.ghostMaterial);
        vfxConstructing.ghostMaterial.color = ghostMatColor;

        vfxConstructing.ghostOverlay.ApplyOverlay(vfxConstructing.ghostMaterial, "VFXConstructing", false);
    }

    public Color GetGhostColor() => ghostMatColor;
}
