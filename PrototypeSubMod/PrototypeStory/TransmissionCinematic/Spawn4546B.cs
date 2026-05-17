using System;
using System.Collections;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic;

public class Spawn4546B : MonoBehaviour, IMaterialModifier
{
    public event Action<GameObject> onEditMaterial;
    
    private const string PlanetPath =
        "EndSequence/rocketship_everything2/___MASTER_EVERYTHING_ELSE_JOINT___/___PLANET_JNT___/x_EndSequence_Planet";

    private bool spawningPlanet;
    private bool spawnedPlanet;
    
    private void Awake()
    {
        if (spawnedPlanet) return;
        
        SpawnPlanet();
    }

    public void SpawnPlanet()
    {
        UWE.CoroutineHost.StartCoroutine(SpawnPlanetAsync());
    }

    private IEnumerator SpawnPlanetAsync()
    {
        if (spawningPlanet) yield break;
        
        spawningPlanet = true;
        
        var task = CraftData.GetPrefabForTechTypeAsync(TechType.RocketBase);
        yield return task;

        var result = task.GetResult();
        var planet = result.transform.Find(PlanetPath).gameObject;
        var planetInstance = Instantiate(planet, transform);
        planetInstance.SetActive(true);
        UWE.Utils.ZeroTransform(planetInstance);
        Destroy(planetInstance.GetComponent<VFXKeepAtDistance>());
        onEditMaterial?.Invoke(planetInstance);

        spawningPlanet = false;
        spawnedPlanet = true;
    }
}