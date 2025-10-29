using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

internal class AutoreferenceSkyApplier : SkyApplier
{
    new private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }
}
