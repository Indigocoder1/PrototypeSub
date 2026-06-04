using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.WormSpawns;

public class ProtoWormSpawnEvent
{
    public static PrefabInfo RegisterEvent(string classID, GameObject prefab, LootDistributionData.BiomeData[] spawnLocations)
    {
        var prefabInfo = PrefabInfo.WithTechType(classID, null, null);

        var customPrefab = new CustomPrefab(prefabInfo);
        
        customPrefab.SetSpawns(spawnLocations);
        customPrefab.RemoveFromCache();

        IEnumerator GetPrefab(IOut<GameObject> prefabOut)
        {
            var instance = UWE.Utils.InstantiateDeactivated(prefab);
            MaterialUtils.ApplySNShaders(instance, modifiers: new DefaultMaterialModifier());
            yield return ProtoMatDatabase.ReplaceVanillaMats(instance);

            prefabOut.Set(instance);
        }
        
        customPrefab.SetGameObject(GetPrefab);
        customPrefab.Register();
        
        return prefabInfo;
    }
}