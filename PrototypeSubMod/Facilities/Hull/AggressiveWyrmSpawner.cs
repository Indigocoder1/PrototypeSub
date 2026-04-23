using System;
using PrototypeSubMod.Prefabs;
using Story;
using System.Collections;
using System.Collections.Generic;
using PrototypeSubMod.PrototypeStory.CalibrationSite;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull;

public class AggressiveWyrmSpawner : MonoBehaviour, IScheduledUpdateBehaviour
{
    private bool wyrmSpawned;
    private bool canSpawn;
    private float minWyrmSpawnDelay = 20f;
    private float maxWyrmSpawnDelay = 40f;

    private void Start()
    {
        CalibrationRunManager.OnCalibrationCompleted += OnCalibrationCompleted;
        canSpawn = true;
    }

    public void ScheduledUpdate()
    {
        if (WaitScreen.IsWaiting) return;

        if (!StoryGoalManager.main.IsGoalComplete("HullFacilityWormTerminalEncy") || 
            !StoryGoalManager.main.IsGoalComplete("StartedCalibrationRun")) return;
        
        var biomeString = Player.main.GetBiomeString();
        bool inVoid = biomeString is "void" or "";
        inVoid |= biomeString.EndsWith("protovoid");

        if (!inVoid)
        {
            canSpawn = true;
        }
        
        if (!inVoid || !canSpawn || wyrmSpawned) return;
        
        var (hitPoint, info) = FindSpawnPoint();
        var point = info.point;
        var normal = info.normal;
        if (!hitPoint)
        {
            const float spawnOffset = 250;
            var playerPos = Player.main.transform.position;
            var dir = (-playerPos.normalized - Vector3.up) / 2;
            dir.Normalize();
            point = playerPos + dir * spawnOffset;
            normal = -dir;
            Plugin.Logger.LogInfo($"Wyrm spawner didn't detect any hits. Resorting to fallback spawn location");
        }
        
        Plugin.Logger.LogInfo($"Entered the void | Spawn point at {point}");
        StartCoroutine(SpawnWyrm(point, normal));
        wyrmSpawned = true;
    }

    private void OnCalibrationCompleted()
    {
        canSpawn = false;
    }

    private IEnumerator SpawnWyrm(Vector3 point, Vector3 normal)
    {
        var random = Random.Range(minWyrmSpawnDelay, maxWyrmSpawnDelay);

        yield return new WaitForSeconds(random);

        var biomeString = Player.main.GetBiomeString();
        bool inVoid = biomeString is "void" or "";
        inVoid |= biomeString.EndsWith("protovoid");

        if (!inVoid) yield return null;
        
        var task = CraftData.GetPrefabForTechTypeAsync(ProtoAggressiveWyrm.prefabInfo.TechType);
        yield return task;

        var prefab = task.GetResult();
        var instance = Instantiate(prefab, point - normal * 10f, Quaternion.LookRotation(normal));
        instance.GetComponent<ProtoAggressiveWorm>().OnDespawn += OnWyrmDespawned;
    }

    private void OnWyrmDespawned()
    {
        wyrmSpawned = false;
    }

    private (bool hit, RaycastHit info) FindSpawnPoint()
    {
        var testDirections = PointsOnSphere(10);
        RaycastHit raycastHit = default;
        bool hit = false;
        foreach (var dir in testDirections)
        {
            hit = Physics.Raycast(Player.main.transform.position, dir,
                out raycastHit, 500, 1 << LayerID.TerrainCollider);
            if (!hit) continue;
            break;
        }

        return (hit, raycastHit);
    }
    
    private Vector3[] PointsOnSphere(int num)
    {
        Vector3[] points = new Vector3[num];
        float increment = Mathf.PI * (3 - Mathf.Sqrt(5));
        float offset = 2f / num;

        for (int i = 0; i < num; i++)
        {
            float y = (i * offset) - 1 + (offset / 2);
            float r = Mathf.Sqrt(1 - (y * y));
            float phi = i * increment;
            float x = Mathf.Cos(phi) * r;
            float z = Mathf.Sin(phi) * r;

            points[i] = new Vector3(x, y, z);
        }

        return points;
    }

    private void OnDestroy()
    {
        CalibrationRunManager.OnCalibrationCompleted -= OnCalibrationCompleted;
    }

    public string GetProfileTag() => "AggressiveWyrmSpawner";
    
    public void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }

    public void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }

    public int scheduledUpdateIndex { get; set; }
}