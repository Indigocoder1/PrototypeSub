using System;
using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using PrototypeSubMod.Facilities;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.FacilityProps;

public static class StoryGoalTerminal
{
    public static void CreateTerminal(string techType, string storyGoalKey)
    {
        var prefabInfo = PrefabInfo.WithTechType(techType);

        var prefab = new CustomPrefab(prefabInfo);

        GameObject GetPrefab()
        {
            var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("Empty");
        
            var obj = UWE.Utils.InstantiateDeactivated(prefab);
            PrefabUtils.AddBasicComponents(obj, prefabInfo.ClassID, prefabInfo.TechType, LargeWorldEntity.CellLevel.Medium);

            var spawner = new GameObject("Spawner");
            spawner.transform.SetParent(obj.transform, false);
            var terminal = spawner.AddComponent<MultipurposeAlienTerminal>();
            spawner.AddComponent<UnlockStoryGoal>().ManualSetup(terminal, storyGoalKey);
            spawner.AddComponent<ApplyDatabaseOnceTerminalSpawned>().terminal = terminal;

            return obj;
        }
        
        prefab.SetGameObject(GetPrefab);

        prefab.Register();
    }

    private class ApplyDatabaseOnceTerminalSpawned : MonoBehaviour
    {
        public MultipurposeAlienTerminal terminal;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => terminal.GetModelSpawned());
            
            terminal.gameObject.EnsureComponent<ApplyMaterialDatabase>();
        }
    }
}