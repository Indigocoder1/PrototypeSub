using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Utility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class ListeningDevice_Craftable
{

    public static PrefabInfo prefabInfo { get; private set; }

    private static CustomPrefab prefab;
    
    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ListeningDevice", null, null, unlockAtStart: true)
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("ListeningDeviceIcon.png"));

        prefab = new CustomPrefab(prefabInfo);
        
        prefab.SetGameObject(GetPrefab);

        prefab.SetRecipe(new RecipeData
        {
            craftAmount = 1,
            Ingredients = new List<Ingredient>
            {
                new(TechType.Titanium, 1)
            }
        }).WithCraftingTime(5f);
        prefab.SetEquipment(EquipmentType.Chip);
        
        prefab.Register();
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefab)
    {
        // var returnPrefab = Plugin.AssetBundle.LoadAsset<GameObject>("ListeningDevice.prefab");
        //
        // returnPrefab.GetComponent<PrefabIdentifier>().ClassId = prefabInfo.TechType.ToString();
        // returnPrefab.GetComponent<TechTag>().type = prefabInfo.TechType;
        // returnPrefab.SetActive(false);
        //
        // var instance = Object.Instantiate(returnPrefab);
        //
        // var task = PrefabDatabase.GetPrefabAsync("31f717b7-b257-4bff-b54b-422bf5008e04");
        //
        // yield return task;
        //
        // if (!task.TryGetPrefab(out var devicePrefab))
        // {
        //     Plugin.Logger.LogError("Failed to get the listening device model using it's ClassID.");
        //     yield break;
        // }
        //
        // var deviceModel = devicePrefab.GetComponentInChildren<MeshRenderer>().gameObject;
        //
        // var modelInstance = Object.Instantiate(deviceModel, instance.transform.GetChild(0));

        yield return new WaitForSeconds(1f);
        
        var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        PrefabUtils.AddBasicComponents(instance, prefabInfo.ClassID, prefabInfo.TechType, LargeWorldEntity.CellLevel.Global);
        MaterialUtils.ApplySNShaders(instance);
        
        prefab.Set(instance);
    }
    
}