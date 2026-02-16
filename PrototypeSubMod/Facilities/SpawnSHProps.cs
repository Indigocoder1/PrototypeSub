using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using PrototypeSubMod.StructureLoading;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.Facilities;

internal class SpawnSHProps : MonoBehaviour, IMaterialModifier
{
    public event Action<GameObject> onEditMaterial;
    
    [SerializeField] private TextAsset[] structures;
    [SerializeField] private Transform parent;
    [Tooltip("Remove large world entity components")]
    [SerializeField] private bool removeLWEs = true;

    private void Start()
    {
        UWE.CoroutineHost.StartCoroutine(SpawnStructures());
    }

    private IEnumerator SpawnStructures()
    {
        foreach (var obj in structures)
        {
            var structure = JsonConvert.DeserializeObject<Structure>(obj.text);
            Dictionary<string, List<Entity>> entities = new();
            foreach (var entity in structure.Entities)
            {
                if (entity == null || entity.classId == null) continue;

                if (!entities.ContainsKey(entity.classId))
                {
                    entities.Add(entity.classId, new List<Entity>());
                }

                var entityList = entities[entity.classId];
                if (!entityList.Contains(entity))
                {
                    entityList.Add(entity);
                }
            }

            yield return SpawnEntities(entities);
        }
    }

    private IEnumerator SpawnEntities(Dictionary<string, List<Entity>> entities)
    {
        foreach (var classID in entities.Keys)
        {
            var prefabTask = PrefabDatabase.GetPrefabAsync(classID);
            yield return prefabTask;

            GameObject prefab = null;
            if (!prefabTask.TryGetPrefab(out prefab)) throw new System.Exception($"Error loading prefab with class ID of {classID}");
            
            var lwe = prefab.GetComponent<LargeWorldEntity>();
            if (lwe) lwe.enabled = false;

            foreach (var entityData in entities[classID])
            {
                var pos = new Vector3(entityData.position.x, entityData.position.y, entityData.position.z);
                var rot = new Quaternion(entityData.rotation.x, entityData.rotation.y, entityData.rotation.z, entityData.rotation.w);
                var scale = new Vector3(entityData.scale.x, entityData.scale.y, entityData.scale.z);
                var instance = Instantiate(prefab, parent);
                instance.transform.position = pos;
                instance.transform.rotation = rot;
                instance.transform.localScale = scale;

                var instanceLWE = instance.GetComponent<LargeWorldEntity>();
                var cellLevel = instanceLWE?.cellLevel;
                if (removeLWEs && instanceLWE)
                {
                    DestroyImmediate(instanceLWE);
                }

                var tt = CraftData.GetTechType(instance.gameObject);
                instance.EnsureComponent<TechTag>().type = tt;
                var identifier = instance.GetComponent<PrefabIdentifier>();
                if (instance.TryGetComponent(out Pickupable _))
                {
                    instance.EnsureComponent<AddIdentifierOnPickup>().SetOriginalValues(identifier.ClassId, identifier.Id, cellLevel);
                }

                DestroyImmediate(identifier);
                instance.SetActive(true);
                
                foreach (var rend in instance.GetComponentsInChildren<Renderer>())
                {
                    onEditMaterial?.Invoke(rend.gameObject);
                }
            }
            
            if (lwe) lwe.enabled = true;
        }
    }
}
