using System;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class EnergyBeamShot : CinematicShot
{
    public override event Action<DeviceCinematicManager> OnShotCompleted;

    public void EndEnergyBeamShot()
    {
        OnShotCompleted?.Invoke(deviceCinematicManager);
    }
}