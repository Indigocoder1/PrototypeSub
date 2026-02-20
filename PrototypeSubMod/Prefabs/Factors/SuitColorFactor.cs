using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Factors;
using Nautilus.Utility;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.Factors;

public static class SuitColorFactor
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("SuitColorFactor")
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("ProtoFactorIcon"));

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("ColorFactor.json"))
            .WithCraftingTime(3f);
        prefab.SetEquipment(Plugin.FactorEquipmentType);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }
    
    private static IEnumerator GetPrefab(IOut<GameObject> prefabOut)
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("GenericFactorModel");
        prefab.SetActive(false);

        var instance = GameObject.Instantiate(prefab);

        yield return new WaitUntil(() => MaterialUtils.IsReady);

        MaterialUtils.ApplySNShaders(instance, modifiers: new ProtoMaterialModifier(3, 0));

        yield return ProtoMatDatabase.ReplaceVanillaMats(instance);
        
        instance.name = "SuitColorFactor";
        instance.AddComponent<ColorFactor>();
        
        prefabOut.Set(instance);
    }
}