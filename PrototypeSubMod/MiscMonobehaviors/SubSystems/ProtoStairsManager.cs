using System;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class ProtoStairsManager : MonoBehaviour
{
    private static readonly int StairsActive = Animator.StringToHash("StairsActive");

    [SerializeField] private SubRoot subRoot;
    [SerializeField] private Animator stairsAnimator;
    [SerializeField] private FMOD_CustomEmitter stairsUpEmitter;
    [SerializeField] private FMOD_CustomEmitter stairsDownEmitter;
    [SerializeField] private Collider lowerCollider;
    [SerializeField] private Collider lowerAreaBoundsCheck;

    private bool queuedNoPlaySfx;
    private bool stairsActive;
    private bool colliderChecks;
    private bool stairsFinishedMoving;
    public bool fireSupressionActive = false;
    
    private void Start()
    {
        Player.main.playerRespawnEvent.AddHandler(gameObject, OnRespawn);
    }

    private void OnEnable()
    {
        queuedNoPlaySfx = true;
        SetStairsActive(Player.main.currentSub != subRoot);
    }

    private void Update()
    {
        if (!colliderChecks || !stairsFinishedMoving) return;

        if (!lowerAreaBoundsCheck.bounds.Contains(Player.main.transform.position))
        {
            lowerCollider.enabled = true;
            colliderChecks = false;
        }
    }

    private void OnRespawn(Player player)
    {
        SetStairsActive(false);
    }

    public void SetStairsActive(bool active)
    {
        stairsFinishedMoving = false;
        stairsAnimator.SetBool(StairsActive, active);
        stairsActive = active;
    }

    public void ToggleStairsActive()
    {
        if (fireSupressionActive) return;
        SetStairsActive(!stairsActive);
    }

    public void ToggleFromBottom()
    {
        SetStairsActive(!stairsActive);
        colliderChecks = true;
        lowerCollider.enabled = false;
    }

    public void OnStairsFinishedMoving()
    {
        stairsFinishedMoving = true;
    }

    // Called by SubRoot.OnPlayerEntered
    public void PlayerEnteredSub()
    {
        if (fireSupressionActive) return;
        SetStairsActive(false);
    }

    // Called by SubRoot.OnPlayerExited
    public void SaveEngineStateAndPowerDown()
    {
        queuedNoPlaySfx = true;
        SetStairsActive(true);
    }

    public void PlayStairsUpSfx()
    {
        if (!queuedNoPlaySfx) stairsUpEmitter.Play();
        queuedNoPlaySfx = false;
    }
    
    public void PlayStairsDownSfx()
    {
        if (!queuedNoPlaySfx) stairsDownEmitter.Play();
        queuedNoPlaySfx = false;
    }
}