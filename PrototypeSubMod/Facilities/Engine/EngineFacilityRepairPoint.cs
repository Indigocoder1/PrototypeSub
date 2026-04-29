using System;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Engine;

public class EngineFacilityRepairPoint : MonoBehaviour
{
    public static event Action OnPointRepaired;
    
    public const int REPAIR_POINTS_COUNT = 4;

    [SerializeField] private FMODAsset[] remainingPointsVoicelines;
    [SerializeField] private FMOD_CustomEmitter onAllSealedSfx;

    private void Start()
    {
        if (Plugin.GlobalSaveData.repairedEngineFacilityPoints.Contains(gameObject.name))
        {
            gameObject.SetActive(false);
        }
    }

    public void OnRepair()
    {
        Plugin.GlobalSaveData.repairedEngineFacilityPoints.Add(gameObject.name);
        int remainingPoints = REPAIR_POINTS_COUNT - Plugin.GlobalSaveData.repairedEngineFacilityPoints.Count;
        PDALog.Add(remainingPointsVoicelines[remainingPoints].path);
        gameObject.SetActive(false);
        if (remainingPoints <= 0)
        {
            onAllSealedSfx.Play();
        }

        OnPointRepaired?.Invoke();
    }
}