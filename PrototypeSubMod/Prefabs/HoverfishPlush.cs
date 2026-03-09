using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Utility;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class HoverfishPlush
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("HoverfishPlush", null, null, "English")
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("HoverfishPlushIcon"));

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetPrefab);
        prefab.SetEquipment(EquipmentType.Hand);
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe($"{prefabInfo.ClassID}.json"))
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Machines")
            .WithCraftingTime(3f);
        prefab.SetPdaGroupCategory(TechGroup.Miscellaneous, TechCategory.Misc);

        CraftDataHandler.SetBackgroundType(prefabInfo.TechType, CraftData.BackgroundType.Blueprint);

        prefab.Register();
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefabOut)
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("HoverfishPlush");
        prefab.SetActive(false);

        var instance = GameObject.Instantiate(prefab);

        yield return new WaitUntil(() => MaterialUtils.IsReady);

        MaterialUtils.ApplySNShaders(instance, modifiers: new ProtoMaterialModifier(3, 0));

        prefabOut.Set(instance);
    }
}
