using System.Collections;
using System.Collections.Generic;
using PrototypeSubMod.Prefabs;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull;

public class AggressiveWyrmSpawner : MonoBehaviour
{
    private bool wasInVoid;
    private bool wyrmSpawned;

    private void Update()
    {
        if (!LargeWorldStreamer.main.IsWorldSettled()) return;
        
        var biomeString = Player.main.GetBiomeString();
        bool inVoid = biomeString is "void" or "";

        if (inVoid != wasInVoid && inVoid && !wyrmSpawned)
        {
            var (hitPoint, info) = FindSpawnPoint();
            var point = info.point;
            var normal = info.normal;
            if (!hitPoint)
            {
                var mainCam = Camera.main.transform;
                var dir = (-mainCam.forward - mainCam.up) / 2;
                dir.Normalize();
                point = Player.main.transform.position + dir * 50f;
                normal = -dir;
                Plugin.Logger.LogInfo($"Didn't detect any hits. Resorting to fallback spawn location");
            }
            
            ErrorMessage.AddError($"Entered the void | Spawn point at {point}");
            Plugin.Logger.LogInfo($"Spawn point at {point}");
            StartCoroutine(SpawnWyrm(point, normal));
        }

        wasInVoid = inVoid;
    }

    private IEnumerator SpawnWyrm(Vector3 point, Vector3 normal)
    {
        wyrmSpawned = true;
        var task = CraftData.GetPrefabForTechTypeAsync(ProtoAggressiveWyrm.prefabInfo.TechType);
        yield return task;

        var prefab = task.GetResult();
        var instance = Instantiate(prefab, point - normal * 10f, Quaternion.LookRotation(normal));
        instance.GetComponent<ProtoAggressiveWorm>().onDespawn += OnWyrmDespawned;
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
                out raycastHit, 100, 1 << LayerID.TerrainCollider);
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
}