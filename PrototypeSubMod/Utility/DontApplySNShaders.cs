using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.Utility;

internal class DontApplySNShaders : MonoBehaviour
{
    [Tooltip("Leave empty to not apply to all materials")]
    public List<Material> blacklistedMaterials;
}
