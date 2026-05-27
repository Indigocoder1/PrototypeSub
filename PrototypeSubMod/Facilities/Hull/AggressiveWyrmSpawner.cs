using PrototypeSubMod.Prefabs;
using Story;
using System.Collections;
using PrototypeSubMod.PrototypeStory;
using PrototypeSubMod.PrototypeStory.CalibrationSite;
using PrototypeSubMod.Registration;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull;

public class AggressiveWyrmSpawner : MonoBehaviour, IScheduledUpdateBehaviour, IProtoTreeEventListener
{
    private bool wyrmSpawned;
    private bool canSpawn;
    private float minWyrmSpawnDelay = 20f;
    private float maxWyrmSpawnDelay = 40f;
    private float calibrationSpawnDelay = 20f;

    private void Start()
    {
        CalibrationRunManager.OnCalibrationCompleted += OnCalibrationCompleted;
        canSpawn = true;
    }

    public void ScheduledUpdate()
    {
        if (WaitScreen.IsWaiting) return;
        
        if (ProtoStoryLocker.StoryEndingActive) return;

        var wyrmActivated = StoryGoalManager.main.IsGoalComplete("HullFacilityWormTerminalEncy");
        var closeToCalibration = AtmosphereDirector.main.GetBiomeOverride() == "protovoid" &&
                                 Vector3.Distance(Player.main.transform.position, CalibrationRunManager.InitialPoint) <
                                 650;
        var inDZMIRunup = AtmosphereDirector.main.GetBiomeOverride() == BiomeRegisterer.DZMIRunupBiome;
        
        // Always spawn the wyrm in the DZMI runup
        if ((!wyrmActivated || !closeToCalibration) && !inDZMIRunup) return;
        
        var biomeString = Player.main.GetBiomeString();
        bool inVoid = biomeString is "void" or "";
        inVoid |= biomeString.EndsWith("protovoid");

        if (!inVoid)
        {
            canSpawn = true;
        }
        
        if (!inVoid || !canSpawn || wyrmSpawned) return;
        
        StartCoroutine(SpawnWyrm());
        wyrmSpawned = true;
    }

    private void OnCalibrationCompleted()
    {
        canSpawn = false;
    }

    private IEnumerator SpawnWyrm()
    {
        var delay = Random.Range(minWyrmSpawnDelay, maxWyrmSpawnDelay);
        if (!StoryGoalManager.main.IsGoalComplete("WyrmFirstEncounterComplete"))
        {
            delay = calibrationSpawnDelay + Random.Range(-2f, 2f);
        }
        
        Plugin.Logger.LogInfo($"Entered the void | Spawning wyrm in {delay} seconds");
        yield return new WaitForSeconds(delay);

        if (WyrmAlreadySpawned())
        {
            wyrmSpawned = true;
            yield break;
        }
        
        var (hitPoint, info) = FindSpawnPoint();
        var point = info.point;
        var normal = info.normal;
        if (!hitPoint)
        {
            const float spawnOffset = 350;
            var playerPos = Player.main.transform.position;
            var dir = (-playerPos.normalized - Vector3.up) / 2;
            dir.Normalize();
            point = playerPos + dir * spawnOffset;
            normal = -dir;
            Plugin.Logger.LogInfo($"Wyrm spawner didn't detect any hits. Resorting to fallback spawn location");
        }

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
    public void OnProtoSerializeObjectTree(ProtobufSerializer serializer) { }

    public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
    {
        var wyrmAlreadyExists = WyrmAlreadySpawned();
        if (wyrmAlreadyExists && wyrmSpawned)
        {
            StopCoroutine(nameof(SpawnWyrm));
        }

        wyrmSpawned = wyrmAlreadyExists;
    }

    private bool WyrmAlreadySpawned()
    {
        return FindObjectsOfType<ProtoAggressiveWorm>().Length > 0;
    }
}