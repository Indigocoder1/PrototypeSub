using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Factors;

public abstract class Factor : MonoBehaviour
{
    public float cooldown { get; protected set; }
    public float duration { get; protected set; }
    
    public abstract void Use();
    public abstract GameInput.Button GetUseButton();
    
    public IEnumerator WaitDuration()
    {
        yield return new WaitForSeconds(duration);
        Disable();
    }

    public abstract void Disable();
}