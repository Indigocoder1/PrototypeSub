using System;
using UnityEngine;

namespace PrototypeSubMod.UI.ProceduralArcGenerator;

public class UICircularMeshApplier : CircularMeshApplier
{
    [SerializeField] private CanvasRenderer canvasRend;
    [SerializeField] private Material material;

    private void OnValidate()
    {
        if (!canvasRend) TryGetComponent(out canvasRend);
    }

    private void Awake()
    {
        // To avoid issues in the Unity Editor
        if (Player.main == null) return;
        
        var shader = Plugin.ShadersAssetBundle.LoadAsset<Shader>(material.shader.name.Split('/')[^1]);
        material.shader = shader;
    }

    public override void UpdateMesh()
    {
        base.UpdateMesh();

        if (!canvasRend) return;

        canvasRend.materialCount = 1;
        canvasRend.SetMaterial(material, 0);
        canvasRend.SetMesh(lastMesh);
    }

    private void OnDisable()
    {
        canvasRend.Clear();
    }

    private void OnEnable()
    {
        UpdateMesh();
    }
}