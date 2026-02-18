using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PrototypeSubMod.MiscMonobehaviors;

public class HandIconSetter : MonoBehaviour
{
    [SerializeField] private HandReticle.IconType iconType = HandReticle.IconType.Hand;
    [SerializeField] private GameInput.Button handButton = GameInput.Button.None;
    [SerializeField] private GameInput.Button handSubscriptButton = GameInput.Button.None;
    [SerializeField] private string handTextKey;
    [SerializeField] private string handSubscriptTextKey;
    [SerializeField] private float maxDistance = -1;
    
    private bool mouseHovered;
    
    public void MouseEnter(BaseEventData data)
    {
        mouseHovered = true;
    }

    public void MouseExit(BaseEventData data)
    {
        mouseHovered = false;
    }

    private void Update()
    {
        if (!mouseHovered) return;
        
        if (maxDistance > 0 && Vector3.Distance(Camera.main.transform.position, transform.position) > maxDistance) return;
        
        HandReticle.main.SetIcon(iconType);
        HandReticle.main.SetText(HandReticle.TextType.Hand, handTextKey, true, handButton);
        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, handSubscriptTextKey, true, handSubscriptButton);
    }
}