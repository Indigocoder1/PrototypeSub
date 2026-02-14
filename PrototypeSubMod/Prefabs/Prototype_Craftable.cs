using System;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Utility;
using SubLibrary.Handlers;
using SubLibrary.Monobehaviors;
using System.Collections;
using System.Reflection;
using Nautilus.Utility.MaterialModifiers;
using SubLibrary.CyclopsReferencers;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class Prototype_Craftable
{
    public static PrefabInfo SubInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo prefabInfo = PrefabInfo.WithTechType("PrototypeSub", null, null, "English")
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("PrototypeIcon"));

        SubInfo = prefabInfo;

        var prefab = new CustomPrefab(prefabInfo);

        prefab.RemoveFromCache();
        prefab.SetGameObject(GetSubPrefab);

        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("PrototypeSub.json"))
            .WithFabricatorType(CraftTree.Type.None)
            .WithCraftingTime(20f);

        prefab.SetPdaGroupCategory(Plugin.PrototypeGroup, Plugin.PrototypeCategory);

        prefab.Register();
    }

    private static IEnumerator GetSubPrefab(IOut<GameObject> prefabOut)
    {
        GameObject model = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("PrototypeSub");

        model.SetActive(false);
        GameObject prototype = GameObject.Instantiate(model);

        yield return SetupProtoGameObject(prototype);

        prefabOut.Set(prototype);
    }

    public static IEnumerator SetupProtoGameObject(GameObject go)
    {
        go.GetComponent<PingInstance>().pingType = Plugin.PrototypePingType;
        go.GetComponent<TechTag>().type = SubInfo.TechType;
        go.GetComponent<PrefabIdentifier>().classId = SubInfo.ClassID;

        if (go.TryGetComponent(out PrefabIdentifier identifier))
        {
            identifier.ClassId = SubInfo.ClassID;
        }

        MaterialUtils.ApplySNShaders(go, modifiers: new ProtoMaterialModifier(10, 0, false));

        yield return ProtoMatDatabase.ReplaceVanillaMats(go);
        
        var type = Type.GetType("Nautilus.Utility.ThunderkitUtilities.ApplySNMaterial, Nautilus, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        var method = type.GetMethod("AssignMaterials", BindingFlags.Public | BindingFlags.Instance);
        foreach (var component in go.GetComponentsInChildren(type, true))
        {
            method.Invoke(component, null);
        }
    }
}
