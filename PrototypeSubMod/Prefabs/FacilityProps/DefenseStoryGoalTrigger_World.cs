using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.FacilityProps;

internal class DefenseStoryGoalTrigger_World
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoDefenseStoryTrigger", null, null, "English");

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetPrefab);
        prefab.SetSpawns(new SpawnLocation(Plugin.DEFENSE_PING_POS));

        prefab.Register();
    }

    private static GameObject GetPrefab()
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("DefenseStoryGoalTrigger");
        prefab.SetActive(false);

        var instance = GameObject.Instantiate(prefab);

        return instance;
    }
}
