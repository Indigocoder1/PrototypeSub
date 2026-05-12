using Nautilus.Handlers;
using Nautilus.Utility;
using PrototypeSubMod.Prefabs;
using Story;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Engine;

public class SecretDoorTrigger : MonoBehaviour
{
    private static readonly int Door = Animator.StringToHash("OpenDoor");
    
    [SerializeField] private Animator animator;
    [SerializeField] private FMOD_CustomEmitter doorOpenSFX;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != Player.main.gameObject) return;

        if (!StoryGoalManager.main.IsGoalComplete("OrionSurgicalRoomTome")) return;
        OpenDoor();
    }

    private void OpenDoor()
    {
        animator.SetTrigger(Door);
        doorOpenSFX.Play();
    }
}