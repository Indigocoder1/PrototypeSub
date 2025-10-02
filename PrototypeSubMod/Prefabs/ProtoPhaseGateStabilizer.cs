using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using PrototypeSubMod.Compatibility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public class ProtoPhaseGateStabilizer
{
    public static PrefabInfo PrefabInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("ProtoPhaseGateStabilizer", null, null)
            .WithIcon(Plugin.AssetBundle.LoadAsset<Sprite>("exosuitgrapplingarmmodule_Upgraded"));

        var prefab = new CustomPrefab(PrefabInfo);
        
        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("ProtoPhaseGateStabilizer.json"))
            .WithCraftingTime(10f);
        prefab.SetGameObject(GetGameObject);

        prefab.Register();
    }

    private static GameObject GetGameObject()
    {
        var prefab = Plugin.AssetBundle.LoadAsset<GameObject>("AlienFramework");
        var instance = GameObject.Instantiate(prefab);

        foreach (var rend in instance.GetComponentsInChildren<Renderer>(true))
        {
            rend.material.color = Color.cyan;
        }
        
        return instance;
    }
}