using System;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class RadiateInRangeCaller : MonoBehaviour
{
    private float timer;
    
    private void Update()
    {
        if (timer < 0.2f)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
            Radiate();
        }
    }

    private void Radiate()
    {
        if (RadiateInRangeManager.RadiationManagers == null) return;
        
        float highestRadiation = 0;
        foreach (var manager in RadiateInRangeManager.RadiationManagers)
        {
            var radAmount = manager.GetRadiationAmount();
            if (radAmount > highestRadiation)
            {
                highestRadiation = radAmount;
            }
        }

        Player.main.SetRadiationAmount(highestRadiation);
    }
}