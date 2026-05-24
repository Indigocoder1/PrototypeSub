using System;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic.Shots;

public class DevicePovShot : CinematicShot
{
    public override event Action<DeviceCinematicManager> OnShotCompleted;

    public void EndPovShot()
    {
        OnShotCompleted?.Invoke(deviceCinematicManager);
    }
}