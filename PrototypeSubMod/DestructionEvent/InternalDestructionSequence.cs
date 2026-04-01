using System;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.Patches;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.DestructionEvent;

internal class InternalDestructionSequence : DestructionSequence
{
    [SerializeField] private Transform playerPos;
    [SerializeField] private InterfloorTeleporter[] teleporters;
    [SerializeField] private GameObject[] teleporterObjects;

    private void Start()
    {
        Player.main.playerDeathEvent.AddHandler(this, OnPlayerDied);
    }

    public override void StartSequence(SubRoot subRoot)
    {
        LeakingRadiation.main.GetComponent<RadiatePlayerInRange>().CancelInvoke(nameof(RadiatePlayerInRange.Radiate));
        foreach (var teleporter in teleporters)
        {
            teleporter.GetComponent<Collider>().enabled = false;
        }
    }

    private void OnPlayerDied(Player player)
    {
        var radiateInRange = LeakingRadiation.main.GetComponent<RadiatePlayerInRange>();
        radiateInRange.InvokeRepeating(nameof(RadiatePlayerInRange.Radiate), 0, 0.2f);
    }
}
