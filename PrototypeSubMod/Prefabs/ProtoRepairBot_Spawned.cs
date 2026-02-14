using Nautilus.Assets;
using PrototypeSubMod.Extensions;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class ProtoRepairBot_Spawned
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoRepairBot", null, null, "English")
            .WithIcon(SpriteManager.Get(TechType.PrecursorDroid));

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefabOut)
    {
        GameObject model = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("ProtoRepairBot");

        model.SetActive(false);
        GameObject prefab = GameObject.Instantiate(model);

        var botTask = CraftData.GetPrefabForTechTypeAsync(TechType.PrecursorDroid);
        yield return botTask;

        var bot = GameObject.Instantiate(botTask.GetResult(), prefab.transform.GetChild(0));
        RemoveBotComponents(ref bot);
        bot.GetComponentInChildren<FMOD_CustomLoopingEmitter>().playOnAwake = false;

        bot.GetComponent<TechTag>().type = prefabInfo.TechType;

        prefab.SetActive(true);
        prefabOut.Set(prefab);
    }

    private static void RemoveBotComponents(ref GameObject gameObject)
    {
        gameObject.RemoveComponentImmediate<PrefabIdentifier>();
        gameObject.RemoveComponentImmediate<FleeOnDamage>();
        gameObject.RemoveComponentImmediate<StayAtLeashPosition>();
        gameObject.RemoveComponentImmediate<WalkBehaviour>();
        gameObject.RemoveComponentImmediate<SplineFollowing>();
        gameObject.RemoveComponentImmediate<Locomotion>();
        gameObject.RemoveComponentImmediate<CaveCrawler>();
        gameObject.RemoveComponentImmediate<CreatureDeath>();
        gameObject.RemoveComponentImmediate<Rigidbody>();
        gameObject.RemoveComponentImmediate<SphereCollider>();
        gameObject.RemoveComponentImmediate<BoxCollider>();
        gameObject.RemoveComponentImmediate<WorldForces>();
        gameObject.RemoveComponentImmediate<CaveCrawlerGravity>();
        gameObject.RemoveComponentImmediate<LiveMixin>();
        gameObject.RemoveComponentImmediate<CreatureFlinch>();
        gameObject.RemoveComponentImmediate<OnSurfaceMovement>();
        gameObject.RemoveComponentImmediate<OnSurfaceTracker>();
        gameObject.RemoveComponentImmediate<RemoveSoundsOnKill>();
        gameObject.RemoveComponentImmediate<CreatureUtils>();
        gameObject.RemoveComponentImmediate<MoveOnSurface>();
        gameObject.RemoveComponentImmediate<CrawlerAttackLastTarget>();
        gameObject.RemoveComponentImmediate<CrawlerAvoidEdges>();
        gameObject.RemoveComponentImmediate<AggressiveWhenSeeTarget>();
        gameObject.RemoveComponentImmediate<MeleeAttack>();
        gameObject.RemoveComponentImmediate<LastTarget>();
        gameObject.RemoveComponentImmediate<CreatureFear>();
    }
}
