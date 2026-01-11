using System;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Factors.Tether;

public class TetherMarkerManager : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private AnimationCurve opacityOverDistance;
    
    private void Start()
    {
        if (Plugin.GlobalSaveData.tetherFactorMarkerLocation == null)
        {
            Destroy(gameObject);
            return;
        }
        
        if (Vector3.Distance(Plugin.GlobalSaveData.tetherFactorMarkerLocation.Value, transform.position) > 0.1f)
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        transform.LookAt(Player.main.transform.position);
        var color = image.color;
        color.a = opacityOverDistance.Evaluate(Vector3.Distance(transform.position, Player.main.transform.position));
        image.color = color;
    }

    private void OnEnable()
    {
        MarkerTetherLogic.onClearTetherMarker += OnClearMarker;
    }
    
    private void OnDisable()
    {
        MarkerTetherLogic.onClearTetherMarker -= OnClearMarker;
    }

    private void OnClearMarker()
    {
        Destroy(gameObject);
    }
}