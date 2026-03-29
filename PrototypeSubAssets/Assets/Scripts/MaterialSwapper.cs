using UnityEngine;

public class MaterialSwapper : MonoBehaviour
{
    [SerializeField] private Material materialFrom;
    [SerializeField] private Material materialTo;
    [SerializeField] private bool swapMaterials;
    
    private void OnDrawGizmosSelected()
    {
        if (!swapMaterials) return;
        swapMaterials = false;

        foreach (var rend in GetComponentsInChildren<Renderer>(true))
        {
            var materials = rend.sharedMaterials;
            for (var i = 0; i < materials.Length; i++)
            {
                if (materials[i] == materialFrom)
                {
                    materials[i] = materialTo;
                }
            }

            rend.sharedMaterials = materials;
        }
    }
}
