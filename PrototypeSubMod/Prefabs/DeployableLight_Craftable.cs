using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Utility;
using SubLibrary.CyclopsReferencers;
using SubLibrary.Handlers;
using SubLibrary.Monobehaviors;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class DeployableLight_Craftable
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("DeployableLight", null, null, "English")
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("LightBeacon_Icon"));

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetPrefab);

        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("DeployableLight.json"))
            .WithCraftingTime(10f);

        prefab.SetEquipment(Plugin.LightBeaconEquipmentType);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);

        prefab.Register();
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefabOut)
    {
        var assetPrefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("DeployableLight");

        assetPrefab.SetActive(false);
        var prefab = UWE.Utils.InstantiateDeactivated(assetPrefab);

        prefab.GetComponent<Pickupable>().isPickupable = false;

        yield return new WaitUntil(() => MaterialUtils.IsReady);

        yield return CyclopsReferenceHandler.EnsureCyclopsReference();

        MaterialUtils.ApplySNShaders(prefab, modifiers: new ProtoMaterialModifier(30f));

        yield return ProtoMatDatabase.ReplaceVanillaMats(prefab);

        foreach (var referencer in prefab.GetComponentsInChildren<ICyclopsReferencer>(true))
        {
            referencer.OnCyclopsReferenceFinished(CyclopsReferenceHandler.CyclopsReference);
        }

        foreach (var item in prefab.GetComponentsInChildren<PrefabModifier>(true))
        {
            item.OnLateMaterialOperation();
            item.OnAsyncPrefabTasksCompleted();
        }

        prefabOut.Set(prefab);
    }
}
