using System;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class OSSpecificMaterial : MonoBehaviour
{
    [SerializeField] private Renderer renderer;
    [SerializeField] private int materialIndex = -1;

    private void OnValidate()
    {
        if (!renderer) TryGetComponent(out renderer);
    }

    private void Awake()
    {
        var mats = renderer.materials;
        if (materialIndex == -1)
        {
            foreach (var material in mats)
            {
                var shader = Plugin.ShadersAssetBundle.LoadAsset<Shader>(material.shader.name.Split('/')[^1]);
                material.shader = shader;
            }
        }
        else
        {
            mats[materialIndex].shader = Plugin.ShadersAssetBundle.LoadAsset<Shader>(mats[materialIndex].shader.name.Split('/')[^1]);
        }

        renderer.materials = mats;
    }
}