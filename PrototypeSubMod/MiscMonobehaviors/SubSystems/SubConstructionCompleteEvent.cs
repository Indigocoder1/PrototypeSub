using System;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class SubConstructionCompleteEvent : MonoBehaviour
{
    public static event Action<GameObject> OnSubConstructed; 
    
    // Called via BroadcastMessage by VFXConstructing
    private void SubConstructionComplete()
    {
        OnSubConstructed?.Invoke(null);
    }
}