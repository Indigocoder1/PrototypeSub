using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmDespawnAction : CreatureAction
{
    [SerializeField] private AggressiveWormAnimator wormAnimator;
    
    private bool performing;
    
    public override float Evaluate(Creature creature, float time)
    {
        return performing ? 100 : 0;
    }
    
    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;
        
        base.Perform(creature, time, deltaTime);
        
        performing = true;

        var dir = Player.main.transform.position.normalized + Vector3.down * 0.5f;
        var point = Player.main.transform.position + dir * 300;
        wormAnimator.SetTravelTarget(point, OnReachedTarget);
        StartCoroutine(TargetRecheck(point));
        Plugin.Logger.LogInfo($"Started despawn action");
    }

    // I don't remember why I added this, but it's probably important so I'll leave it for now
    private IEnumerator TargetRecheck(Vector3 targetPoint)
    {
        yield return new WaitForSeconds(1f);
        wormAnimator.SetTravelTarget(targetPoint, OnReachedTarget);
    }

    private void OnReachedTarget()
    {
        var biomeString = Player.main.GetBiomeString();
        bool inVoid = biomeString is "void" or "";

        if (!inVoid)
        {
            Destroy(gameObject);
        }
        else
        {
            performing = false;
        }
    }

    public bool IsPerforming() => performing;
}