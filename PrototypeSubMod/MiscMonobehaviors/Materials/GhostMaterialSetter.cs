using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

internal class GhostMaterialSetter : MonoBehaviour
{
    [SerializeField] private VFXConstructing vfxConstructing;
    [SerializeField] private Color ghostMatColor;

    private Material ghostMat;
    
    // Called via Unity Event on CustomSubVFXConstructing
    public void OnConstructionStarted()
    {
        if (ghostMat != null)
        {
            Destroy(ghostMat);
        }
        
        ghostMat = new Material(vfxConstructing.ghostMaterial);
        vfxConstructing.ghostMaterial = ghostMat;
        vfxConstructing.ghostMaterial.color = ghostMatColor;

        vfxConstructing.ghostOverlay.ApplyOverlay(vfxConstructing.ghostMaterial, "VFXConstructing", false);
    }

    public Color GetGhostColor() => ghostMatColor;
    
    private void OnDestroy()
    {
        Destroy(ghostMat);
    }
}
