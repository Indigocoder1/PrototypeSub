using System;
using UnityEngine;

namespace PrototypeSubMod.Factors.Tether;

public class TetherMarkerManager : MonoBehaviour
{
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