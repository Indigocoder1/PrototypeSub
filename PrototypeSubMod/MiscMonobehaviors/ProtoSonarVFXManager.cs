using System;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class ProtoSonarVFXManager : MonoBehaviour
{
    private Shader shader;
    private float pingDistance;
    private float sonarNearPlane = 20f;
    private float borderStartPoint = 0.7f;
    private float transitonDuration = 2f;
    private Color outlineColor = new (0f, 0.419f, 0);
    private Color crossHatchColor = new (0.3f, 0.72f, 0.07f);
    
    private Material material;
    private bool activated;
    private float currentActivationTime = 1;

    private void Awake()
    {
        shader = Plugin.ShadersAssetBundle.LoadAsset<Shader>("ProtoSonarEffect");
    }

    public void SetActivated(bool activated)
    {
        this.activated = activated;
        currentActivationTime = 0;
    }

    public void ToggleActivated()
    {
        SetActivated(!activated);
    }

    private void Update()
    {
        if (!(currentActivationTime < 1)) return;
        
        currentActivationTime += Time.deltaTime / transitonDuration;
        float progress = activated ? EaseInCubic(currentActivationTime) : 1 - EaseInCubic(currentActivationTime);
        pingDistance = progress;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (shader == null) return;
        
        if (material == null)
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
        }

        material.SetFloat("_ProtoSonarPingDistance", pingDistance);
        material.SetFloat("_SonarNearPlane", sonarNearPlane);
        material.SetFloat("_BorderStartPoint", borderStartPoint);
        material.SetColor("_SonarOutlineColor", outlineColor);
        material.SetColor("_CrossHatchColor", crossHatchColor);
        Graphics.Blit(source, destination, material);
    }

    private float EaseOutCubic(float x)
    {
        return 1 - Mathf.Pow(1 - x, 3);
    }

    private float EaseInCubic(float x)
    {
        return x * x * x;
    }
}