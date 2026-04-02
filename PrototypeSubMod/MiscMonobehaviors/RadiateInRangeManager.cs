using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class RadiateInRangeManager : MonoBehaviour
{
    public static List<RadiateInRangeManager> RadiationManagers;
    
    private RadiatePlayerInRange radiatePlayerInRange;
    
    private void Start()
    {
        radiatePlayerInRange = GetComponent<RadiatePlayerInRange>();
        radiatePlayerInRange.CancelInvoke(nameof(RadiatePlayerInRange.Radiate));
    }

    public float GetRadiationAmount()
    {
        var prevAmount = Player.main.radiationAmount;
        radiatePlayerInRange.Radiate();
        var amount = Player.main.radiationAmount;
        Player.main.SetRadiationAmount(prevAmount);
        return amount;
    }

    private void OnEnable()
    {
        RadiationManagers ??= new List<RadiateInRangeManager>();
        RadiationManagers.Add(this);
    }

    private void OnDisable()
    {
        RadiationManagers.Remove(this);
    }
}