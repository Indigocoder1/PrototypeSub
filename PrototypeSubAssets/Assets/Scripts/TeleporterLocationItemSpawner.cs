using PrototypeSubMod.Teleporter;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TeleporterLocationItemSpawner : MonoBehaviour
{
    public bool spawnPrefabs;
    public bool setAllDirty;
    public float realToMapScaleRatio;
    public ProtoTeleporterIDManager teleporterIDManager;
    public Transform itemsParent;
    public GameObject teleporterPrefab;

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        HandleSpawning();
        HandleDirtying();
    }

    private void HandleSpawning()
    {
        if (!spawnPrefabs) return;
        spawnPrefabs = false;

        foreach (var positionData in TeleporterPositionHandler.TeleporterPositions)
        {
            var pos = positionData.Value.teleportPosition;
            Vector2 flatPos = new Vector2(pos.x, pos.z);
            var obj = PrefabUtility.InstantiatePrefab(teleporterPrefab, itemsParent) as GameObject;
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = flatPos * realToMapScaleRatio;
            var item = obj.GetComponent<TeleporterLocationItem>();

            bool host = positionData.Key.Contains("M");
            item.SetInfo(positionData.Key, host, teleporterIDManager);
            EditorUtility.SetDirty(item);
        }
    }

    private void HandleDirtying()
    {
        if (!setAllDirty) return;
        setAllDirty = false;

        foreach (var item in GetComponentsInChildren<TeleporterLocationItem>(true))
        {
            EditorUtility.SetDirty(item);
            EditorUtility.SetDirty(item.GetComponentInChildren<Button>());
            EditorUtility.SetDirty(item.gameObject);
        }
    }
#endif
}
