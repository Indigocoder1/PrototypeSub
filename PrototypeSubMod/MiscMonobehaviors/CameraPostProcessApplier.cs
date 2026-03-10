using System.Collections.Generic;
using Nautilus.Extensions;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace PrototypeSubMod.MiscMonobehaviors;

public class CameraPostProcessApplier : MonoBehaviour
{
    [SerializeField] private Camera applicationCamera;
    [SerializeField] private bool addWBOIT;

    private List<WaterClipProxy> waterClipProxies = new();
    
    private void OnValidate()
    {
        if (!applicationCamera) TryGetComponent(out applicationCamera);
    }

    private void Start()
    {
        var mainCamera = Camera.main;

        gameObject.SetActive(false);
        gameObject.EnsureComponent<ColorCorrection>().CopyComponent(mainCamera.GetComponent<ColorCorrection>());
        gameObject.EnsureComponent<LensWater>().CopyComponent(mainCamera.GetComponent<LensWater>());
        gameObject.EnsureComponent<LensWaterController>().CopyComponent(mainCamera.GetComponent<LensWaterController>());
        gameObject.EnsureComponent<WaterscapeVolumeOnCamera>().CopyComponent(mainCamera.GetComponent<WaterscapeVolumeOnCamera>());
        if (addWBOIT)
        {
            var wboit = gameObject.EnsureComponent<WBOIT>().CopyComponent(mainCamera.GetComponent<WBOIT>());
            wboit.camera = applicationCamera;
            wboit.guiCamera = null;
        }
        var behavior = gameObject.EnsureComponent<PostProcessingBehaviour>().CopyComponent(mainCamera.GetComponent<PostProcessingBehaviour>());
        behavior.m_Camera = applicationCamera;
        gameObject.SetActive(true);
    }

    public void DisableWaterClipProxies()
    {
        waterClipProxies.Clear();
        var proxies = FindObjectsOfType<WaterClipProxy>();
        foreach (var proxy in proxies)
        {
            if (proxy.gameObject.activeSelf)
            {
                waterClipProxies.Add(proxy);
                proxy.gameObject.SetActive(false);
            }
        }
    }
    
    public void EnableWaterClipProxies()
    {
        foreach (var proxy in waterClipProxies)
        {
            proxy.gameObject.SetActive(true);
        }
    }
}