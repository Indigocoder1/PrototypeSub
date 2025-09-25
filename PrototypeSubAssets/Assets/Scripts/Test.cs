using PrototypeSubMod.Teleporter;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{
    public bool spawnPrefabs;
    public float realToMapScaleRatio;
    public ProtoTeleporterIDManager teleporterIDManager;
    public Transform itemsParent;
    public GameObject teleporterPrefab;

    #if UNITY_EDITOR
    private void OnDrawGizmos()
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
        }
    }
#endif
}
