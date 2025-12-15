using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class RandomDoorSelector : MonoBehaviour
{
    [SerializeField] private BearingRoomTeleporterManager originRoomManager;
    [SerializeField] private BearingRoomTeleporterManager targetRoomManager;
    [SerializeField] private BearingSymbolIndicator bearingIndicator;
    [SerializeField] private BearingReferenceSymbol[] possibleBearings;
    [SerializeField] private BearingTeleporterDoor[] possibleDoors;
    [SerializeField] private BearingTeleporterDoor teleportToDoor;

    private void Start()
    {
        var index = Mathf.RoundToInt(Random.Range(0, (possibleBearings.Length - 1) * 10f) / 10f);

        var doorFrom = possibleDoors[index];
        bearingIndicator.SetReferenceSymbol(possibleBearings[index]);
        originRoomManager.LinkTeleporters(doorFrom, teleportToDoor);
        originRoomManager.GeneratePreviewImage(doorFrom, teleportToDoor);
        targetRoomManager.GeneratePreviewImage(teleportToDoor, doorFrom);
    }
}