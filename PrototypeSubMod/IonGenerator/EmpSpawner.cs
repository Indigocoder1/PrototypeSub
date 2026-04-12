using System;
using System.Collections;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using UnityEngine;

namespace PrototypeSubMod.IonGenerator;

internal class EmpSpawner : MonoBehaviour, IMaterialModifier
{
    public event Action<GameObject> onEditMaterial;
    
    [SerializeField] private Transform empSpawnPos;
    [SerializeField] private float empLifetime;
    [SerializeField] private AnimationCurve blastRadius;
    [SerializeField] private AnimationCurve blastHeight;

    private GameObject empPrefab;

    private void Start()
    {
        UWE.CoroutineHost.StartCoroutine(RetrievePrefab());
    }

    private IEnumerator RetrievePrefab()
    {
        CoroutineTask<GameObject> crabsquidTask = CraftData.GetPrefabForTechTypeAsync(TechType.CrabSquid);

        yield return crabsquidTask;

        GameObject crabsquid = crabsquidTask.result.Get();
        var empAttack = crabsquid.GetComponent<EMPAttack>();
        
        empPrefab = UWE.Utils.InstantiateDeactivated(empAttack.ammoPrefab);
    }

    public void FireEMP(float disableElectronicsTime)
    {
        var newEMP = Instantiate(empPrefab, empSpawnPos.position, empSpawnPos.rotation, transform);
        newEMP.SetActive(true);
        var empBlast = newEMP.GetComponent<EMPBlast>();
        
        empBlast.disableElectronicsTime = disableElectronicsTime;
        empBlast.lifeTime = empLifetime;
        empBlast.blastRadius = blastRadius;
        empBlast.blastHeight = blastHeight;
        Destroy(empBlast.GetComponentInChildren<VFXLerpColor>());

        onEditMaterial?.Invoke(newEMP.gameObject);
    }

    public Transform GetSpawnPos() => empSpawnPos;
    public float GetLifetime() => empLifetime;

    public float GetFinalRadius()
    {
        return blastRadius.Evaluate(1);
    }
}
