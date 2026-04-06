using Nautilus.Assets;
using System.Collections;
using UnityEngine;
using UWE;

namespace PrototypeSubMod.Prefabs;

internal static class SmashedDisplayCase_World
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("SmashedDisplayCase", null, null, "English");

        var prefab = new CustomPrefab(prefabInfo);

        prefab.SetGameObject(GetPrefab);

        prefab.Register();
    }

    private static IEnumerator GetPrefab(IOut<GameObject> prefabOut)
    {
        var prefab = Plugin.GeneralAssetBundle.LoadAsset<GameObject>("SmashedDisplayCase");
        prefab.SetActive(false);

        var instance = UWE.Utils.InstantiateDeactivated(prefab);

        var displayCaseTask = PrefabDatabase.GetPrefabAsync("d0fea4da-39f2-47b4-aece-bb12fe7f9410");
        yield return displayCaseTask;

        GameObject casePrefab = null;
        if (!displayCaseTask.TryGetPrefab(out casePrefab)) throw new System.Exception("Error retrieving display case prefab!");

        var bottomRend = casePrefab.transform.Find("Precursor_lab_container_01/Precursor_lab_container_01_bottom").GetComponent<Renderer>();
        var glassRend = casePrefab.transform.Find("Precursor_lab_container_01/Precursor_lab_container_01_glass").GetComponent<Renderer>();
        var topRend = casePrefab.transform.Find("Precursor_lab_container_01/Precursor_lab_container_01_top").GetComponent<Renderer>();

        instance.transform.Find("DestroyedContainerSeperated/Precursor_lab_container_01_bottom").GetComponent<Renderer>().material = bottomRend.material;
        instance.transform.Find("DestroyedContainerSeperated/Precursor_lab_container_01_glass").GetComponent<Renderer>().material = glassRend.material;
        instance.transform.Find("DestroyedContainerSeperated/Precursor_lab_container_01_top").GetComponent<Renderer>().material = topRend.material;

        prefabOut.Set(instance);
    }
}
