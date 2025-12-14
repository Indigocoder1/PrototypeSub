using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class RandomDoorSelector : MonoBehaviour
{
    [SerializeField] private BearingRoomTeleporterManager teleporterManager;
    [SerializeField] private BearingSymbolIndicator bearingIndicator;
    [SerializeField] private BearingReferenceSymbol[] possibleBearings;
    [SerializeField] private BearingTeleporterDoor[] possibleDoors;
    [SerializeField] private BearingTeleporterDoor teleportToDoor;

    private void Start()
    {
        var index = Random.Range(0, possibleBearings.Length - 1);
        bearingIndicator.SetReferenceSymbol(possibleBearings[index]);
        teleporterManager.LinkTeleporters(possibleDoors[index], teleportToDoor);
    }
}