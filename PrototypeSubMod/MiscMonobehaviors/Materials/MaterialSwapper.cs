using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class MaterialSwapper : MonoBehaviour
{
    [SerializeField] private Material materialFrom;
    [SerializeField] private Material materialTo;

    public void SwapMaterials(bool reverse = false)
    {
        foreach (var rend in GetComponentsInChildren<Renderer>(true))
        {
            var materials = rend.sharedMaterials;
            for (var i = 0; i < materials.Length; i++)
            {
                if (materials[i] == (reverse ? materialTo : materialFrom))
                {
                    materials[i] = reverse ? materialFrom : materialTo;
                }
            }

            rend.sharedMaterials = materials;
        }
    }
}
