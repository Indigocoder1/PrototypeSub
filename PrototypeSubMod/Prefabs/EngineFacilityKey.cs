using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using PrototypeSubMod.Compatibility;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

internal class EngineFacilityKey
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("EngineFacilityKey", null, null)
            .WithIcon(Plugin.GeneralAssetBundle.LoadAsset<Sprite>("EngineTabletIcon"));

        var prefab = new CustomPrefab(prefabInfo);
        var cloneTemplate = new CloneTemplate(prefabInfo, TechType.PrecursorKey_Purple);
        cloneTemplate.ModifyPrefab += gameObject =>
        {
            Texture2D replacementGlyph = Plugin.GeneralAssetBundle.LoadAsset<Texture2D>("EngineGlyph");
            var rend1 = gameObject.transform.Find("Model/Rig_J/precursor_key_C_02_symbol_05").GetComponent<Renderer>();
            var rend2 = gameObject.transform.Find("ViewModel/Rig_J/precursor_key_C_02_symbol_05").GetComponent<Renderer>();

            var tempMats = rend1.materials;
            tempMats[1].SetTexture("_MainTex", replacementGlyph);
            tempMats[1].SetTexture("_SpecTex", replacementGlyph);
            tempMats[1].SetTexture("_Illum", replacementGlyph);
            rend1.materials = tempMats;

            tempMats = rend2.materials;
            tempMats[1].SetTexture("_MainTex", replacementGlyph);
            tempMats[1].SetTexture("_SpecTex", replacementGlyph);
            tempMats[1].SetTexture("_Illum", replacementGlyph);
            rend2.materials = tempMats;

            gameObject.GetComponent<Collider>().isTrigger = false;
        };

        prefab.SetGameObject(cloneTemplate);
        prefab.SetPdaGroupCategory(Plugin.ProtoFabricatorGroup, Plugin.ProtoFabricatorCatgeory);

        prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe("EngineFacilityKey.json"));

        prefab.Register();
    }
}
