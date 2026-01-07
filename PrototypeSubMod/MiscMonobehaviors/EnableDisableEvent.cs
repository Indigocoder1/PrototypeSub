using System;
using UnityEngine;
using UnityEngine.Events;

namespace PrototypeSubMod.MiscMonobehaviors;

public class EnableDisableEvent : MonoBehaviour
{
    [SerializeField] private UnityEvent onEnable;
    [SerializeField] private UnityEvent onDisable;

    private void OnEnable()
    {
        onEnable?.Invoke();
    }

    private void OnDisable()
    {
        onDisable?.Invoke();
    }
}