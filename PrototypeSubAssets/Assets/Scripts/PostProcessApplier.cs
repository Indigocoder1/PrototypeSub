using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcessApplier : MonoBehaviour
{
    [SerializeField] private Shader shader;
    [Range(0,1)]
    [SerializeField] private float pingDistance;
    [SerializeField] private Color outlineColor;
    [SerializeField] private Color crossHatchColor;
    [SerializeField] private bool resetMaterial;
    [Range(0,1)]
    [SerializeField] private float sonarNearPlane;
    
    private Material _material;
    
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_material == null || resetMaterial)
        {
            _material = new Material(shader);
            _material.hideFlags = HideFlags.DontSave;
            resetMaterial = false;
        }

        _material.SetFloat("_SonarPingDistance", pingDistance);
        _material.SetFloat("_SonarNearPlane", sonarNearPlane);
        _material.SetColor("_SonarOutlineColor", outlineColor);
        _material.SetColor("_CrossHatchColor", crossHatchColor);
        Graphics.Blit(source, destination, _material);
    }
}
