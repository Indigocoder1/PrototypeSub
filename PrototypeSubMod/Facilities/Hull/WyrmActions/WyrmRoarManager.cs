using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmRoarManager : MonoBehaviour
{
    [SerializeField] private FMOD_CustomEmitter emitter;
    [SerializeField] private FMODAsset[] closeRoarSounds;
    [SerializeField] private FMODAsset[] mediumRoarSounds;
    [SerializeField] private FMODAsset[] farRoarSounds;
    [SerializeField] private float mediumDistanceThreshold;
    [SerializeField] private float farDistanceThreshold;

    private void OnValidate()
    {
        farDistanceThreshold = Mathf.Max(mediumDistanceThreshold + 1, farDistanceThreshold);
    }

    public void PlayRoar(Vector3 listeningPosition)
    {
        var assets = GetRangedAssets(listeningPosition);
        emitter.asset = assets[Random.Range(0, assets.Length - 1)];
        emitter.Play();
    }

    private FMODAsset[] GetRangedAssets(Vector3 listeningPosition)
    {
        float dist = Vector3.Distance(transform.position, listeningPosition);
        
        if (dist < mediumDistanceThreshold) return closeRoarSounds;
        if (dist < farDistanceThreshold) return mediumRoarSounds;
        return farRoarSounds;
    }
}