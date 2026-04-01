using PrototypeSubMod.IonGenerator;
using System.Collections;
using SubLibrary.SubFire;
using UnityEngine;

namespace PrototypeSubMod.DestructionEvent;

internal class ExternalDestructionSequence : DestructionSequence
{
    [SerializeField] private Transform warpOutSpawnPos;
    [SerializeField] private EmpSpawner empSpawner;
    [SerializeField] private float disableElectronicsTime;

    private GameObject warpOutFX;

    private void Start()
    {
        UWE.CoroutineHost.StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        var task = CraftData.GetPrefabForTechTypeAsync(TechType.Warper);
        yield return task;

        var result = task.GetResult();
        var warper = result.GetComponent<Warper>();
        warpOutFX = warper.warpOutEffectPrefab;
    }

    public override void StartSequence(SubRoot subRoot)
    {
        empSpawner.FireEMP(disableElectronicsTime);

        var fx = Instantiate(warpOutFX, warpOutSpawnPos.position, warpOutSpawnPos.rotation);
        fx.transform.localScale = Vector3.one * 10f;
        
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
        
        subRoot.subWarning = false;
        subRoot.fireSuppressionState = false;
        subRoot.silentRunning = false;
        subRoot.BroadcastMessage("NewAlarmState");
    }
}
