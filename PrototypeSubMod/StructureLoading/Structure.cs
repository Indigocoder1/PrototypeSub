using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nautilus.Handlers;
using Newtonsoft.Json;
using UnityEngine;

namespace PrototypeSubMod.StructureLoading;

public class Structure
{
    public Entity[] Entities { get; private set; }
    
    public bool IsSorted { get; private set; }
    
    public Structure(Entity[] entities)
    {
        Entities = entities;
    }
    
    public static Structure LoadFromFile(string jsonFilePath)
    {
        return JsonConvert.DeserializeObject<Structure>(File.ReadAllText(jsonFilePath));
    }
    
    public static Structure LoadFromBundle(string fileName)
    {
        return JsonConvert.DeserializeObject<Structure>(Plugin.GeneralAssetBundle.LoadAsset<TextAsset>(fileName).text);
    }

    public static IEnumerator RegisterFromBundle(string fileName)
    {
        var assetOp = Plugin.GeneralAssetBundle.LoadAsset<TextAsset>(fileName);
        var structure = JsonConvert.DeserializeObject<Structure>(assetOp.text);

        yield return structure.RegisterStructure(20);
    }
    
    public void SortByPriority()
    {
        Entities = Entities.OrderBy(( entity) => entity.priority).ToArray();
        IsSorted = true;
    }
}

public static class StructureExtensions
{
    private static List<string> registeredIds = new();
    
    public static void RegisterStructure(this Structure structure)
    {
        structure.SortByPriority();
        foreach (var entity in structure.Entities)
        {
            RegisterEntity(entity);
        }
    }

    public static IEnumerator RegisterStructure(this Structure structure, int entitiesPerFrame)
    {
        structure.SortByPriority();
        yield return new WaitForEndOfFrame();

        int entityCount = 0;
        foreach (var entity in structure.Entities)
        {
            RegisterEntity(entity);
            entityCount++;
            if (entityCount > entitiesPerFrame)
            {
                entityCount = 0;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public static void RegisterEntity(this Entity entity)
    {
        if (string.IsNullOrEmpty(entity.classId)) return;
            
        if (registeredIds.Contains(entity.id)) return;
            
        CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(entity.classId, entity.position.ToVector3(),
            entity.rotation.ToQuaternion(), entity.scale.ToVector3(),
            (obj) =>
            {
                obj.GetComponent<UniqueIdentifier>().Id = entity.id;
            }));

        registeredIds.Add(entity.id);
    }
}