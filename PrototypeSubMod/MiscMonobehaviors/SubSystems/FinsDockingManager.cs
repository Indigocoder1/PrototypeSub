using System;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class FinsDockingManager : MonoBehaviour
{
    private static readonly int DockingPrep = Animator.StringToHash("DockingPrep");
    private static readonly int DockingGrab = Animator.StringToHash("DockingGrab");

    [SerializeField] private Animator[] dockingPrepFins;

    private ProtoFinsManager finsManager;

    private void Start()
    {
        finsManager = GetComponent<ProtoFinsManager>();
    }

    // These are called via ForwardMessageTrigger
    public void LaunchbayAreaEnter(GameObject nearby)
    {
        var vehicle = UWE.Utils.GetComponentInHierarchy<Vehicle>(nearby.gameObject);
        if (!vehicle || vehicle.docked) return;

        SetDockingPrep(true);
    }
    
    public void LaunchbayAreaExit(GameObject nearby)
    {
        var vehicle = UWE.Utils.GetComponentInHierarchy<Vehicle>(nearby.gameObject);
        if (!vehicle || vehicle.docked) return;

        SetDockingPrep(false);
        finsManager.ResetFinAnimations();
    }

    public void SetDockingPrep(bool prepActive)
    {
        foreach (var animator in dockingPrepFins)
        {
            animator.SetBool(DockingPrep, prepActive);
        }
    }

    public void StartGrabAnimation()
    {
        foreach (var animator in dockingPrepFins)
        {
            animator.SetTrigger(DockingGrab);
        }
    }

    public ProtoFinsManager GetFinsManager() => finsManager;
}