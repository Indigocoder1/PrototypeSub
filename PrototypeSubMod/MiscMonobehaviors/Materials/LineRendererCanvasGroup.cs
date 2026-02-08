using System;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class LineRendererCanvasGroup : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private LineRenderer lineRenderer;

    private void OnValidate()
    {
        if (!lineRenderer) TryGetComponent(out lineRenderer);
    }

    private void LateUpdate()
    {
        var alpha = canvasGroup.alpha;
        var endColor = lineRenderer.endColor;
        endColor.a = alpha;
        lineRenderer.endColor = endColor;

        var startColor = lineRenderer.startColor;
        startColor.a = alpha;
        lineRenderer.startColor = startColor;
    }
}