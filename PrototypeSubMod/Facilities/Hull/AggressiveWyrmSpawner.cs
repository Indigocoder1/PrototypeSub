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
            var hitInfo = FindSpawnPoint();
            ErrorMessage.AddError($"Entered the void | Spawn point at {hitInfo.point}");
            Plugin.Logger.LogInfo($"Spawn point at {hitInfo.point}");
            StartCoroutine(SpawnWyrm(hitInfo));
        }

        wasInVoid = inVoid;
    }

    private IEnumerator SpawnWyrm(RaycastHit hitInfo)
    {
        wyrmSpawned = true;
        var task = CraftData.GetPrefabForTechTypeAsync(ProtoAggressiveWyrm.prefabInfo.TechType);
        yield return task;

        var prefab = task.GetResult();
        var instance = Instantiate(prefab, hitInfo.point - hitInfo.normal * 10f, Quaternion.LookRotation(hitInfo.normal));
        instance.GetComponent<ProtoAggressiveWorm>().onDespawn += OnWyrmDespawned;
    }

    private void OnWyrmDespawned()
    {
        wyrmSpawned = false;
    }

    private RaycastHit FindSpawnPoint()
    {
        var testDirections = PointsOnSphere(10);
        RaycastHit raycastHit = default;
        foreach (var dir in testDirections)
        {
            bool hit = Physics.Raycast(Player.main.transform.position, dir,
                out raycastHit, 100, 1 << LayerID.TerrainCollider);
            if (!hit) continue;
            break;
        }

        return raycastHit;
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