using System;
using System.Collections;
using System.Collections.Generic;
using PrototypeSubMod.Utility;
using Story;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull;

public class WormSpawnEvent : MonoBehaviour
{
    private const float MIN_TIME_BETWEEN_SPAWNS = 600;

    private static readonly Vector3 QepLocation = new Vector3(465.67f, -109.81f, 1216.69f);
    
    [SaveStateReference(float.MinValue)] 
    public static float TimeWormsEnabled;
    
    [SaveStateReference(float.MinValue)]
    private static float _timeNextSpawn = float.MinValue;

    [SaveStateReference]
    private static List<Vector3> _activeSpawnLocations;
    
    [SaveStateReference]
    private static GameObject _digInFX;
    [SaveStateReference]
    private static GameObject _digOutFX;

    [SerializeField] private PassiveWormAnimator wormAnimator;
    [SerializeField] private GameObject disableObjects;
    [SerializeField] private Transform raycastOrigin;
    
    [Header("SFX")]
    [SerializeField] private FMOD_CustomEmitter breachSurfaceSFX;
    [SerializeField] private FMOD_CustomLoopingEmitter swimLoopSFX;
    [SerializeField] private FMOD_CustomLoopingEmitter rumbleFarSFX;
    [SerializeField] private FMOD_CustomLoopingEmitter rumbleCloseSFX;
    
    private bool spawnedDigOutParticles;
    private bool wormActive;
    private bool hasSpawned;
    private bool calledDestroy;
    private bool stoppedCoroutines;
    private int particleCount;
    private float timeNextParticles = float.MinValue;
    private float timeOffset;

    private void Awake()
    {
        disableObjects.SetActive(false);
        UWE.CoroutineHost.StartCoroutine(TryRetrieveFXPrefabs());

        float speed = wormAnimator.GetRotationSpeed();
        bool rotationNeedsFlip = Vector3.Dot(Quaternion.AngleAxis(speed, transform.right) * transform.forward, Vector3.up) <
                    Vector3.Dot(Quaternion.AngleAxis(-speed, transform.right) * transform.forward, Vector3.up);
        if (rotationNeedsFlip)
        {
            wormAnimator.SetRotationSpeed(-speed);
        }
    }

    private IEnumerator Start()
    {
        timeOffset = (float)gameObject.GetHashCode() / int.MaxValue;

        yield return new WaitForEndOfFrame();
        
        if (_activeSpawnLocations == null) _activeSpawnLocations = new();

        if (_activeSpawnLocations.Contains(transform.position) || Vector3.Distance(transform.position, QepLocation) < 250)
        {
            Destroy(gameObject);
            calledDestroy = true;
            yield break;
        }
        
        _activeSpawnLocations.Add(transform.position);
    }

    private void Update()
    {
        if (!StoryGoalManager.main.IsGoalComplete("HullFacilityActivateWorm")) return;

        if (calledDestroy) return;
        
        float time = Time.time + timeOffset;
        bool timeAllows = time >= _timeNextSpawn && Time.time > TimeWormsEnabled + MIN_TIME_BETWEEN_SPAWNS;
        disableObjects.SetActive(timeAllows || wormActive);
        if (timeAllows && !wormActive && !hasSpawned)
        {
            _timeNextSpawn = time + MIN_TIME_BETWEEN_SPAWNS;
            wormActive = true;
            hasSpawned = true;
        }

        var main = LargeWorldStreamer.main;
        Vector3 jitter = Random.onUnitSphere * 3;

        var cell1 = main.streamerV2.octreesStreamer.
            GetOctree(main.GetBlock(raycastOrigin.position + jitter) / main.blocksPerTree);
        var cell2 = main.streamerV2.octreesStreamer.
                GetOctree(main.GetBlock(raycastOrigin.position - jitter) / main.blocksPerTree);
        bool hit1 = cell1 != null && !cell1.IsEmpty();
        bool hit2 = cell2 != null && !cell2.IsEmpty();
        bool hitTerrain = hit1 && hit2;

        if (wormAnimator.DoneRotating() && !calledDestroy)
        {
            Destroy(gameObject);
            calledDestroy = true;
        }
        
        if (wormAnimator.HeadIsDisabled())
        {
            if (!stoppedCoroutines)
            {
                StopAllCoroutines();
                stoppedCoroutines = true;
            }
            
            swimLoopSFX.Stop();
            breachSurfaceSFX.Stop();
            rumbleFarSFX.Stop();
            rumbleCloseSFX.Stop();

            return;
        }
        
        float relativeWormLength = wormAnimator.GetDistanceMoved() / wormAnimator.GetRotationDuration();
        if (hitTerrain && Time.time > timeNextParticles && relativeWormLength > 0.2f && !wormAnimator.HeadIsDisabled())
        {
            const int maxParticleCount = 3;
            if (particleCount <= maxParticleCount)
            {
                float normalizedParticleCount = (float)particleCount / maxParticleCount;
                float particleDuration = Mathf.Lerp(wormAnimator.GetRotationDuration() / 1.5f,
                    wormAnimator.GetRotationDuration(),Mathf.InverseLerp(0.5f, 1f, normalizedParticleCount));
                
                StartCoroutine(SpawnPrefabRepeating(_digInFX, raycastOrigin.position, particleDuration,
                    0.5f));
            
                timeNextParticles = Time.time + Mathf.Lerp(1f, 3f, normalizedParticleCount);
            }
        }

        float sqrDistToHead = (raycastOrigin.position - transform.position).sqrMagnitude;
        if (!spawnedDigOutParticles && sqrDistToHead < 400 && !wormAnimator.HeadIsDisabled())
        {
            UWE.CoroutineHost.StartCoroutine(SpawnPrefabRepeating(_digOutFX, raycastOrigin.position,
                wormAnimator.GetRotationDuration(), 0.5f));
            breachSurfaceSFX.Play();
            swimLoopSFX.Play();
            
            spawnedDigOutParticles = true;
        }

        HandleRumbleAudio();
    }

    private void HandleRumbleAudio()
    {
        const float farDistThreshold = 30 * 30;
        if ((Player.main.transform.position - raycastOrigin.position).sqrMagnitude > farDistThreshold)
        {
            rumbleFarSFX.Play();
            rumbleCloseSFX.Stop();
        }
        else
        {
            rumbleCloseSFX.Play();
            rumbleFarSFX.Stop();
        }
    }

    private IEnumerator SpawnPrefabRepeating(GameObject prefab, Vector3 point, float totalDuration, float particleDuration)
    {
        particleCount++;
        for (int i = 0; i < Mathf.CeilToInt(totalDuration / particleDuration); i++)
        {
            for (int j = 0; j < 2; j++)
            {
                var instance = Instantiate(prefab, point + Random.onUnitSphere * 2, Random.rotation);
                instance.transform.localScale = Vector3.one * 2f;
            }
            yield return new WaitForSeconds(particleDuration);
        }

        particleCount--;
    }

    private IEnumerator TryRetrieveFXPrefabs()
    {
        if (_digInFX && _digOutFX) yield break;

        var prefabTask = CraftData.GetPrefabForTechTypeAsync(TechType.Sandshark);
        yield return prefabTask;

        var prefab = prefabTask.GetResult();
        SandShark sandShark = prefab.GetComponent<SandShark>();
        _digInFX = sandShark.digInEffect;
        _digOutFX = sandShark.digOutEffect;
    }
    
    private void OnDestroy()
    {
        _activeSpawnLocations.Remove(transform.position);
    }

    public static void ResetSpawnTimer()
    {
        _timeNextSpawn = 0;
    }
}