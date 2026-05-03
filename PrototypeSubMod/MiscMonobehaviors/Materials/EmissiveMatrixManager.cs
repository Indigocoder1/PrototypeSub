using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

[ExecuteInEditMode]
public class EmissiveMatrixManager : MonoBehaviour
{
    private static readonly int EmissiveAreaTransformMatrix = Shader.PropertyToID("_EmissiveAreaTransformMatrix");
    private static readonly int MatrixPosOffset = Shader.PropertyToID("_MatrixPosOffset");

    [SerializeField] private Transform rootObject;

    private void Update()
    {
        var oldPos = rootObject.transform.position;
        rootObject.transform.position = Vector3.zero;
        Shader.SetGlobalMatrix(EmissiveAreaTransformMatrix, rootObject.worldToLocalMatrix);
        Shader.SetGlobalVector(MatrixPosOffset, -oldPos);
        rootObject.transform.position = oldPos;
    }
}