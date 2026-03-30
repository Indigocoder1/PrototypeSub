using System;
using PrototypeSubMod.MiscMonobehaviors.Emission;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class EmissionColorRegistrar : MonoBehaviour
{
    [SerializeField] private EmissionColorController colorController;
    [SerializeField] private Color emissionColor;
    [SerializeField] private float intensity = 1f;
    [SerializeField] private int priority;

    private void Start()
    {
        colorController.RegisterTempColor(this,
            new EmissionColorController.EmissionRegistrarData(emissionColor * intensity, priority));
    }
}