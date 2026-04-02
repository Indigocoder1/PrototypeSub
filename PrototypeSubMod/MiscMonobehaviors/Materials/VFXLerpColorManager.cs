using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.Materials;

public class VFXLerpColorManager : MonoBehaviour
{
    private static readonly int HydrolockEnabled = Animator.StringToHash("HydrolockEnabled");
    [SerializeField] private Animator hydrolockAnimator;
    [SerializeField] private VFXLerpColor[] lerpColors;
    
    public void OnConstructionCompleted()
    {
        // If disabling doors, don't continue
        if (!hydrolockAnimator.GetBool(HydrolockEnabled)) return;

        PlayLerp(true);
    }

    public void OnConstructionStarted()
    {
        // If doors were just enabled, reset the colors
        if (hydrolockAnimator.GetBool(HydrolockEnabled))
        {
            foreach (var lerpColor in lerpColors)
            {
                foreach (var mat in lerpColor.mats)
                {
                    mat.color = lerpColor.colorEnd;
                }
            }
            
            return;
        }

        PlayLerp(false);
    }

    public void PlayLerp(bool reversed)
    {
        foreach (var lerpColor in lerpColors)
        {
            if (lerpColor.mats[0].shader.name == "Standard")
            {
                Destroy(lerpColor.mats[0]);
                lerpColor.Awake();
            }
            
            lerpColor.reverse = reversed;
            lerpColor.Play();
        }
    }
}