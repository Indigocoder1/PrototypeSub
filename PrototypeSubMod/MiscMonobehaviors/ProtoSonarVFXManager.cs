using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class ProtoSonarVFXManager : MonoBehaviour
{
    private Shader _shader;
    private float _pingDistance;
    private float _sonarNearPlane = 0.044f;
    private float _borderStartPoint = 0.252f;
    private Color _outlineColor = new (0.388f, 1, 0);
    private Color _crossHatchColor = new (0.3f, 0.72f, 0.07f);
    
    private Material _material;

    private void Awake()
    {
        _shader = Plugin.ShadersAssetBundle.LoadAsset<Shader>("ProtoSonarEffect");
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_shader == null) return;
        
        if (_material == null)
        {
            _material = new Material(_shader);
            _material.hideFlags = HideFlags.DontSave;
        }

        _material.SetFloat("_SonarPingDistance", _pingDistance);
        _material.SetFloat("_SonarNearPlane", _sonarNearPlane);
        _material.SetFloat("_BorderStartPoint", _borderStartPoint);
        _material.SetColor("_SonarOutlineColor", _outlineColor);
        _material.SetColor("_CrossHatchColor", _crossHatchColor);
        Graphics.Blit(source, destination, _material);
    }
}