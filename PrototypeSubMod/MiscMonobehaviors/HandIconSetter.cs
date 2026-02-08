using System;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class HandIconSetter : MonoBehaviour
{
    [SerializeField] private HandReticle.IconType iconType = HandReticle.IconType.Hand;
    [SerializeField] private string handTextKey;
    [SerializeField] private string handSubscriptTextKey;
    [SerializeField] private float maxDistance = -1;
    
    private bool mouseHovered;
    
    public void MouseEnter()
    {
        mouseHovered = true;
    }

    public void MouseExit()
    {
        mouseHovered = false;
    }

    private void Update()
    {
        if (!mouseHovered) return;
        
        if (maxDistance > 0 && Vector3.Distance(Camera.main.transform.position, transform.position) > maxDistance) return;
        
        HandReticle.main.SetIcon(iconType);
        HandReticle.main.SetText(HandReticle.TextType.Hand, handTextKey, false);
        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, handSubscriptTextKey, false);
    }
}