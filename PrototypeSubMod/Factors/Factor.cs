using UnityEngine;

namespace PrototypeSubMod.Factors;

public abstract class Factor : MonoBehaviour
{
    public float cooldown { get; protected set; }
    public float duration { get; protected set; }

    protected bool inUse { get; private set; }
    
    public virtual void Use()
    {
        inUse = true;
    }

    public virtual void StopUse()
    {
        inUse = false;
    }

    public virtual void OnEquipped() { }
    public virtual void OnUnequipped() { }
    public virtual void UpdateFactor() { }

    public bool InUse()
    {
        return inUse;
    }
    public abstract GameInput.Button GetUseButton();
}