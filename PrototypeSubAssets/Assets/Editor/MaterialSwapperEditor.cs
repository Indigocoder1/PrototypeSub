using PrototypeSubMod.MiscMonobehaviors.Materials;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MaterialSwapper))]
public class MaterialSwapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Swap Materials"))
        {
            ((MaterialSwapper)target).SwapMaterials();
        }
    }
}
