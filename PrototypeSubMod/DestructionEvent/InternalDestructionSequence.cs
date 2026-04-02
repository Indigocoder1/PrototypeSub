using System.Collections;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using SubLibrary.SubFire;
using UnityEngine;

namespace PrototypeSubMod.DestructionEvent;

internal class InternalDestructionSequence : DestructionSequence
{
    [SerializeField] private Transform playerPos;
    [SerializeField] private InterfloorTeleporter[] teleporters;
    [SerializeField] private GameObject[] teleporterObjects;

    private SubRoot subRoot;
    
    private void Start()
    {
        Player.main.playerDeathEvent.AddHandler(this, OnPlayerDied);
    }

    public override void StartSequence(SubRoot subRoot)
    {
        this.subRoot = subRoot;
        LeakingRadiation.main.GetComponent<RadiatePlayerInRange>().CancelInvoke(nameof(RadiatePlayerInRange.Radiate));
        foreach (var teleporter in teleporters)
        {
            teleporter.GetComponent<Collider>().enabled = false;
        }
    }

    private void OnPlayerDied(Player player)
    {
        if (subRoot == null) return;
        
        var radiateInRange = LeakingRadiation.main.GetComponent<RadiatePlayerInRange>();
        radiateInRange.InvokeRepeating(nameof(RadiatePlayerInRange.Radiate), 0, 0.2f);

        subRoot.GetComponent<Stabilizer>().enabled = false;
        subRoot.worldForces.underwaterGravity = 3;
        
        foreach (var room in subRoot.GetComponentsInChildren<SubRoom>(true))
        {
            var nodes = room.GetSpawnNodes();
            foreach (var node in nodes)
            {
                for (int i = 0; i < node.childCount; i++)
                {
                    Destroy(node.GetChild(i).gameObject);
                }
            }
        }

        UWE.CoroutineHost.StartCoroutine(DisableAlarmDelayed(subRoot));

        subRoot = null;
    }

    private IEnumerator DisableAlarmDelayed(SubRoot subRoot)
    {
        yield return new WaitForSeconds(1f);
        
        subRoot.subWarning = false;
        subRoot.fireSuppressionState = false;
        subRoot.silentRunning = false;
        subRoot.BroadcastMessage("NewAlarmState");
    }
}
