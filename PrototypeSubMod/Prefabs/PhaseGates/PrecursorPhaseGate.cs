using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.PhaseGates;

internal class PrecursorPhaseGate
{
    public static PrefabInfo prefabInfo { get; private set; }

    private static CustomPrefab prefab;

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("PrecursorPhaseGate");

        prefab = new CustomPrefab(prefabInfo);
        prefab.SetGameObject(GetPrefab);
        prefab.Register();
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefab)
    {
        var returnPrefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("PhaseGate.prefab");
        
        if(returnPrefab == null)
            Plugin.Logger.LogError("Failed to load the PrecursorPhaseGate prefab.");

        returnPrefab.GetComponent<PrefabIdentifier>().ClassId = prefabInfo.TechType.ToString();
        returnPrefab.GetComponent<TechTag>().type = prefabInfo.TechType;

        returnPrefab.SetActive(false);
        
        var instance = Object.Instantiate(returnPrefab);
        
        MaterialUtils.ApplySNShaders(instance, modifiers: new DefaultMaterialModifier());

        yield return ProtoMatDatabase.ReplaceVanillaMats(instance);
        
        prefab.Set(instance);
    }
}