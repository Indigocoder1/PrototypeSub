using System.Collections;
using Nautilus.Assets;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.Prefabs.AlienBuildingBlock;

internal class WarperRemnant : RelicBlock
{
    public static PrefabInfo prefabInfo { get; private set; }

    private static CustomPrefab prefab;
    
    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("WarperRemnant").WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("WarperRemnantIcon.png"));
        prefab = new CustomPrefab(prefabInfo);
        
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }
    
    private static IEnumerator GetPrefab(IOut<GameObject> prefab)
    {
        CraftData.PreparePrefabIDCache();
        yield return new WaitForEndOfFrame();

        var task = PrefabDatabase.GetPrefabAsync("09bc9a07-7680-4ddf-9ba2-a7da5e7b3287");
        yield return task;

        if (!task.TryGetPrefab(out var relicBlock))
        {
            Plugin.Logger.LogError("Failed to load the RelicBlock prefab.");
            yield break;
        }
        
        var returnPrefab = Plugin.AssetBundle.LoadAsset<GameObject>("WarperRemnant");
        
        if(returnPrefab == null)
            Plugin.Logger.LogError("Failed to load the WarperRemnant prefab.");

        var instance = UWE.Utils.InstantiateDeactivated(returnPrefab);
        
        var meshRenderer = relicBlock.GetComponentInChildren<MeshRenderer>(true);
        meshRenderer.gameObject.SetActive(true);
        Plugin.Logger.LogInfo($"Rend = {meshRenderer}");
        Plugin.Logger.LogInfo($"Instance = {instance}");
        Plugin.Logger.LogInfo($"Child 0 = {instance.transform.GetChild(0)}");
        
        var relicInstance = GameObject.Instantiate(meshRenderer.gameObject, instance.transform.GetChild(0));

        var relicMat = relicInstance.GetComponent<MeshRenderer>().materials[0];
        
        relicMat.SetFloat(ShaderPropertyID._GlowStrength, 0f);
        relicMat.SetFloat(ShaderPropertyID._GlowStrengthNight, 0f);
        
        prefab.Set(instance);
    }

    //April fools comment.
    public static IEnumerator TrySpawnBiome(Vector3 position, string biome)
    {
        bool inRenderDistance = (position - MainCamera.camera.transform.position).sqrMagnitude < 256f;

        if (!inRenderDistance)
            yield break;
        
        var existingBlocks = Object.FindObjectsOfType<BuildingBlockManager>();

        int existingBlocksInBiome = 0;
        foreach (var block in existingBlocks)
        {
            if(block.spawnBiome.Equals(biome) && block.warperBlock)
                existingBlocksInBiome++;
        }

        if (existingBlocksInBiome >= 5)
            yield break;
        
        var task = CraftData.GetPrefabForTechTypeAsync(prefabInfo.TechType);
        yield return task;
        
        var blockPrefab = task.GetResult();
        
        var spawnedBlock = Object.Instantiate(blockPrefab, position, Quaternion.identity);

        spawnedBlock.GetComponent<Rigidbody>().isKinematic = false;
        spawnedBlock.GetComponent<BuildingBlockManager>().warperBlock = true;
        
        Plugin.Logger.LogDebug($"Warper spawned InactiveBuildingBlock at [{position}]");
    }
}