using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using System.IO;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class PrecursorIngot_Craftable
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("Proto_PrecursorIngot", null, null, "English")
            .WithSizeInInventory(new Vector2int(2, 2))
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("AlienFramework_Icon"));

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetPrefab);

        prefab.SetRecipeFromJson(Path.Combine(Plugin.RecipesFolderPath, "Normal", "Proto_PrecursorIngot.json"))
                .WithCraftingTime(10f);

        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);

        prefab.Register();
    }

    private static GameObject GetPrefab()
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("AlienFramework");
        prefab.SetActive(false);

        var instance = GameObject.Instantiate(prefab);
        return instance;
    }
}
