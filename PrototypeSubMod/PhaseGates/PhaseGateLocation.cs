using UnityEngine;

namespace PrototypeSubMod.PhaseGates;

public struct PhaseGateLocation
{
    public Vector3 Position;
    public Vector3 TeleporterForward;

    public PhaseGateLocation(Vector3 position, Vector3 teleporterForward)
    {
        Position = position;
        TeleporterForward = teleporterForward;
    }
}