using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using PrototypeSubMod.Compatibility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class SurvivorPDA2
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoSurvivorPDA2", null, null);

        var prefab = new CustomPrefab(prefabInfo);

        var cloneTemplate = new CloneTemplate(prefabInfo, "c6f6fe72-e16e-4b00-8df2-6b4e1a3533f4");
        cloneTemplate.ModifyPrefab += gameObject =>
        {
            gameObject.GetComponent<StoryHandTarget>().goal.key = "SurvivorPDA2";
        };

        prefab.SetGameObject(cloneTemplate);

        prefab.Register();
    }
}
