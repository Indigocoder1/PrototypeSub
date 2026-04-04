using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;

namespace PrototypeSubMod.Prefabs.FacilityProps;

public static class ObstructionRockLoadFar
{
    public static PrefabInfo prefabInfo;

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ObstructionRockLoadFar", null, null, "English");

        var prefab = new CustomPrefab(prefabInfo);
        var cloneTemplate = new CloneTemplate(prefabInfo, "fa986d5a-0cf8-4c63-af9f-8c36acd5bea4");
        cloneTemplate.ModifyPrefab += gameObject =>
        {
            var lwe = gameObject.GetComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Far;
        };

        prefab.SetGameObject(cloneTemplate);
        prefab.RemoveFromCache();

        prefab.Register();
    }
}