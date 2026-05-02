using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class EmissiveAreaManager : MonoBehaviour
{
    private static readonly int EmissiveAreaTransformMatrix = Shader.PropertyToID("_EmissiveAreaTransformMatrix");
    
    [SerializeField] private Transform rootObject;

    private void Update()
    {
        Shader.SetGlobalMatrix(EmissiveAreaTransformMatrix, rootObject.worldToLocalMatrix);
    }
}