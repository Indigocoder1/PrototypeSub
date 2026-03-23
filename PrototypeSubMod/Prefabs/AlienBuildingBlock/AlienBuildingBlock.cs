using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using PrototypeSubMod.Compatibility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.AlienBuildingBlock;

internal class AlienBuildingBlock : RelicBlock
{
    public static PrefabInfo prefabInfo { get; private set; }

    private static CustomPrefab prefab;
    
    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("AlienBuildingBlock", null, null)
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("AlienBuildingBlockIcon.png"));
        
        prefab = new CustomPrefab(prefabInfo);
        
        prefab.SetGameObject(GetPrefab);
        
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("AlienBuildingBlock.json")).WithCraftingTime(3f);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);

        CraftDataHandler.SetEatingSound(prefabInfo.TechType, "event:/player/use_first_aid");
        prefab.Register();
    }
    
    private static IEnumerator GetPrefab(IOut<GameObject> prefab)
    {
        var returnPrefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("AlienBuildingBlock.prefab");
        
        if(returnPrefab == null)
            Plugin.Logger.LogError("Failed to load the AlienBuildingBlock prefab.");
        
        returnPrefab.GetComponent<PrefabIdentifier>().ClassId = prefabInfo.TechType.ToString();
        returnPrefab.GetComponent<TechTag>().type = prefabInfo.TechType;
        returnPrefab.SetActive(false);

        var instance = Object.Instantiate(returnPrefab);
        
        var rootRelic = new TaskResult<GameObject>();
        yield return GetRelicBlockModel(rootRelic);

        var relicInstance = Object.Instantiate(rootRelic.Get(), instance.transform.GetChild(0));

        var relicMat = relicInstance.GetComponent<MeshRenderer>().materials[0];
        
        relicMat.SetColor(ShaderPropertyID._GlowColor, new Color(0.2858f, 0.9523f, 1f));
        relicMat.SetFloat(ShaderPropertyID._GlowStrength, 20f);
        relicMat.SetFloat(ShaderPropertyID._GlowStrengthNight, 15f);
        
        prefab.Set(instance);
    }
}