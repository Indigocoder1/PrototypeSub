using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Utility;
using System.Collections;
using Nautilus.Handlers;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class IonPrism_Craftable
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("IonPrism", null, null)
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("IonPrism_Icon"));

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetPrefab);

        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("IonPrism.json"))
            .WithCraftingTime(10f);

        prefab.SetEquipment(Plugin.PrototypePowerType);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);

        CraftDataHandler.SetPickupSound(prefabInfo.TechType, "event:/loot/pickup_precursorioncrystal");

        prefab.Register();
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefabOut)
    {
        var assetPrefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("IonPrism_Prefab");

        var prefab = UWE.Utils.InstantiateDeactivated(assetPrefab);
        MaterialUtils.ApplySNShaders(prefab, modifiers: new ProtoMaterialModifier(3));

        yield return ProtoMatDatabase.ReplaceVanillaMats(prefab);
        prefabOut.Set(prefab);
    }
}
