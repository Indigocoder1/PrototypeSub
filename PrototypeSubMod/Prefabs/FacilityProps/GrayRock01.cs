using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.FacilityProps;

public static class GrayRock01
{
    public static PrefabInfo prefabInfo;

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("GrayRock01", null, null);
        
        var prefab = new CustomPrefab(prefabInfo);
        var cloneTemplate = new CloneTemplate(prefabInfo, "8d13d081-431e-4ed5-bc99-2b8b9fabe9c2");
        cloneTemplate.ModifyPrefab += gameObject =>
        {
            var rend = gameObject.transform.Find("Group1").GetComponent<Renderer>();
            var rock01 = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("rock_01");
            var rock01Normal = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("rock_01_normal");
            rend.material.SetTexture("_CapTexture", rock01);
            rend.material.SetTexture("_CapBumpMap", rock01Normal);
            rend.material.SetTexture("_SideTexture", rock01);
            rend.material.SetTexture("_SideBumpMap", rock01Normal);
        };

        prefab.SetGameObject(cloneTemplate);

        prefab.Register();
    }
}