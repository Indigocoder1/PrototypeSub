using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using UnityEngine;

namespace PrototypeSubMod.Prefabs.FacilityProps;

internal class DeactivatedTeleporter_World
{
    public static PrefabInfo prefabInfo { get; private set; }

    public static void Register()
    {
        prefabInfo = PrefabInfo.WithTechType("ProtoDeactivatedTeleporter", null, null, "English");

        var prefab = new CustomPrefab(prefabInfo);
        var cloneTemplate = new CloneTemplate(prefabInfo, "9a5ff289-4b82-4b9c-b9f6-d60f46dd2d7a");
        cloneTemplate.ModifyPrefab += gameObject =>
        {
            var teleporter = gameObject.GetComponent<PrecursorTeleporter>();
            GameObject.Destroy(teleporter);
        };


        prefab.RemoveFromCache();
        prefab.SetGameObject(cloneTemplate);

        prefab.Register();
    }
}
