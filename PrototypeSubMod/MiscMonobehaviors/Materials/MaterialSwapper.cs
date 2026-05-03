using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class MaterialSwapper : MonoBehaviour
{
    [SerializeField] private GameObject swapRoot;
    [SerializeField] private Material materialFrom;
    [SerializeField] private Material materialTo;
    [SerializeField] private bool swapSharedMaterials;

    public void SwapMaterials()
    {
        foreach (var rend in swapRoot.GetComponentsInChildren<Renderer>(true))
        {
            var materials = swapSharedMaterials ? rend.sharedMaterials : rend.materials;
            for (var i = 0; i < materials.Length; i++)
            {
                if (GetBaseMatName(materials[i].name) == materialFrom.name)
                {
                    materials[i] = materialTo;
                }
            }

            if (swapSharedMaterials)
            {
                rend.sharedMaterials = materials;
            }
            else
            {
                rend.materials = materials;
            }
        }
    }

    private string GetBaseMatName(string instancedName)
    {
        return instancedName.Replace("(Instance)", string.Empty).TrimEnd();
    }
}
