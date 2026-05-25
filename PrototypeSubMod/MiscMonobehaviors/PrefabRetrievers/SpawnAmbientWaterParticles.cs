using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.PrefabRetrievers;

public class SpawnAmbientWaterParticles : MonoBehaviour
{
    private void Start()
    {
        var playerFXSpawner = Player.main.transform.Find("camPivot/camRoot/camOffset/pdaCamPivot/SpawnPlayerFX")
            .GetComponent<PrefabSpawn>();
        var particlesSpawner =
            playerFXSpawner.prefab.transform.Find("WaterParticlesSpawner").GetComponent<PrefabSpawn>();

        var instance = Instantiate(particlesSpawner.prefab, transform);
        Destroy(instance.GetComponent<AmbientParticles>());
        instance.GetComponent<ParticleSystem>().Play();
    }
}