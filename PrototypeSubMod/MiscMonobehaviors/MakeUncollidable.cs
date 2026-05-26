using System;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class MakeUncollidable : MonoBehaviour
{
    [SerializeField] private int layer1;
    [SerializeField] private int layer2;
    
    private void Start()
    {
        Physics.IgnoreLayerCollision(layer1, layer2);
    }
}