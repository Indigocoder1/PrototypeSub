using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using UnityEngine;

namespace PrototypeSubMod.Patches;

public class SilenceCyclopsGrab_Patches
{
    public static bool SpawnPilotWindowLeakFX_Prefix(GameObject cyclops, ref GameObject __result)
    {
        bool hasComponent = cyclops.TryGetComponent(out ProtoRigidbodyFreezer a);
        if (hasComponent)
        {
            __result = new GameObject("Leak FX dummy");
        }
        return !hasComponent;
    }
}