using UnityEngine;

namespace PrototypeSubMod.Puzzles.BearingPuzzle;

public class LinkBearingDoors : MonoBehaviour
{
    [SerializeField] private BearingTeleporterDoor doorFrom;
    [SerializeField] private BearingTeleporterDoor doorTo;
    [SerializeField] private BearingRoomTeleporterManager roomFrom;

    public void LinkDoors()
    {
        roomFrom.LinkTeleporters(doorFrom, doorTo);
        roomFrom.GeneratePreviewImage(doorFrom, doorTo);
    }
}