using Newtonsoft.Json.Linq;
using PrototypeSubMod.Facilities.Hull.WyrmActions;
using PrototypeSubMod.LightDistortionField;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrototypeSubMod.Facilities.Hull;

public class ProtoAggressiveWorm : Creature
{
    public event Action onDespawn;

    [SerializeField] private WyrmDespawnAction despawnAction;
    [SerializeField] private ProtoWormSpineManager spineManager;
    [SerializeField] private Color passiveEmissionColor;
    [SerializeField] private Color aggressiveEmissionColor;
    [SerializeField] private GameObject headObject;
    [SerializeField] private float secondsInVoidForAggression;
    [SerializeField] private float attackRadius = 5f;
    [SerializeField] private float attackDamage = 200f;
    [SerializeField] private PlayerCinematicController cinematicController;

    [Header("SFX")]
    [SerializeField] private FMOD_CustomEmitter aggroOnSfx;
    [SerializeField] private FMOD_CustomEmitter aggroOffSfx;
    [SerializeField] private FMOD_CustomEmitter consumePlayerSfx;
    [SerializeField] private WyrmRoarManager roarManager;
    [SerializeField] private float minRoarInterval = 30f;
    [SerializeField] private float maxRoarInterval = 60f;

    private Renderer[] headRenderers;
    private List<Renderer>[] segmentRenderers;
    private VFXElectricArcs[] electricArcs;
    private float secondsInVoid;
    private bool wasAggressive;
    private int segmentCount;
    private int numSegmentsAggressiveLastFrame;
    private bool hasDamagedTarget;
    private float damageTimer;
    private bool playerBeingEaten;

    public override void Start()
    {
        base.Start();

        roarManager.PlayRoar(Player.main.transform.position);
        liveMixin.invincible = true;
        GetComponent<Rigidbody>().useGravity = false;
        StartCoroutine(RetrieveSegmentRends());
        headRenderers = headObject.GetComponentsInChildren<Renderer>();
        segmentCount = spineManager.GetSpineSegmentCount();

        StartCoroutine(RandomRoar());
    }

    private IEnumerator RandomRoar()
    {
        var secondsUntilRoar = UnityEngine.Random.Range(minRoarInterval, maxRoarInterval);

        yield return new WaitForSeconds(secondsUntilRoar);

        roarManager.PlayRoar(Player.main.transform.position);

        if (despawnAction.IsPerforming())
        {
            // ErrorMessage.AddError("Stopping roar.");
            yield break;
        }
        StartCoroutine(RandomRoar());
    }
    
    public override bool TryStartAction(CreatureAction action)
    {
        if (despawnAction.IsPerforming()) return false;
        
        return base.TryStartAction(action);
    }

    private IEnumerator RetrieveSegmentRends()
    {
        yield return new WaitUntil(() => spineManager.GetSpawned());
        yield return new WaitUntil(() => spineManager.GetChild(0).GetComponentInChildren<VFXElectricArcs>(true));
        
        var segmentCount = spineManager.GetSpineSegmentCount();
        segmentRenderers = new List<Renderer>[segmentCount];
        electricArcs = new VFXElectricArcs[segmentCount - 1];
        for (int i = 0; i < segmentCount; i++)
        {
            var child = spineManager.GetChild(i);
            segmentRenderers[i] = child.GetComponentsInChildren<Renderer>(true).ToList();
            if (i == segmentCount - 1) continue;

            electricArcs[i] = child.GetComponentInChildren<VFXElectricArcs>(true);
        }
    }

    private void Update()
    {
        var biomeString = Player.main.GetBiomeString();
        bool inVoid = biomeString is "void" or "";
        inVoid |= biomeString.EndsWith("protovoid");

        var colliders = Physics.OverlapSphere(transform.position, attackRadius);
        foreach (var col in colliders)
        {
            var mixin = col.GetComponentInParent<LiveMixin>();
            var player = Player.main;

            if (mixin == null || hasDamagedTarget) continue;
            if (mixin.GetComponentInChildren<SubRoot>()) continue;
            if (mixin.GetComponentInParent<ProtoAggressiveWorm>() != null) continue;

            if (mixin == player.liveMixin && !player.currentSub && !playerBeingEaten)
            {
                StartCoroutine(EatPlayer());
                hasDamagedTarget = true;
                break;
            }

            mixin.TakeDamage(attackDamage, transform.position, DamageType.Drill, gameObject);
            hasDamagedTarget = true;
            damageTimer = 2f;
            break;
        }

        if (damageTimer > 0) damageTimer -= Time.deltaTime;
        
        if (damageTimer <= 0f)
        {
            hasDamagedTarget = false;
        }

        if (secondsInVoid < secondsInVoidForAggression && inVoid)
        {
            secondsInVoid += Time.deltaTime;
        }
        else if (secondsInVoid > 0 && !inVoid)
        {
            secondsInVoid -= Time.deltaTime;
        }

        if (secondsInVoid <= 0 && !inVoid && !despawnAction.IsPerforming())
        {
            foreach (var action in actions)
            {
                action.StopPerform(this, Time.time);
                action.SendMessage("OverrideStopPerform");
            }
            
            despawnAction.Perform(this, Time.time, 0);
        }

        if (IsAggressive() != wasAggressive)
        {
            if (IsAggressive())
            {
                aggroOnSfx.Play();
            }
            else
            {
                aggroOffSfx.Play();
            }
        }

        var segmentsAggressive = Mathf.Clamp((int)(secondsInVoid / secondsInVoidForAggression * segmentCount), 0, segmentCount);

        if (segmentsAggressive != numSegmentsAggressiveLastFrame)
        {
            UpdateSegmentColors(segmentsAggressive);
        }
        
        numSegmentsAggressiveLastFrame = segmentsAggressive;
        wasAggressive = IsAggressive();
    }

    private void UpdateSegmentColors(int segmentsAggressive)
    {
        for (var i = 0; i < segmentCount; i++)
        {
            if (segmentRenderers == null) break;
            
            if (!spineManager.transform.GetChild(i).gameObject.activeSelf) continue;
            
            var isAggressive = i >= segmentCount - segmentsAggressive;
            foreach (var rend in segmentRenderers[i])
            {
                UpdateRendererEmissionColor(rend, isAggressive);
            }
            
            if (i == segmentCount - 1 || electricArcs == null) continue;
            UpdateArcColors(electricArcs[i], isAggressive);
        }

        foreach (var headRenderer in headRenderers)
        {
            UpdateRendererEmissionColor(headRenderer, segmentsAggressive == segmentCount);
        }
    }

    private void UpdateRendererEmissionColor(Renderer rend, bool aggressive)
    {
        var color = aggressive ? aggressiveEmissionColor : passiveEmissionColor;
        var materials = rend.materials;
        foreach (var mat in materials)
        {
            mat.SetColor(ShaderPropertyID._GlowColor, color);
        }
        rend.materials = materials;
    }

    private void UpdateArcColors(VFXElectricArcs arcs, bool aggressive)
    {
        var color = aggressive ? aggressiveEmissionColor : passiveEmissionColor;
        foreach (var line in arcs.lines)
        {
            line.line.material.color = color;
        }
    }

    public void ResetAggression(float timeToBecomeAggressive)
    {
        secondsInVoid = 0;
        secondsInVoidForAggression = timeToBecomeAggressive;
    }

    public bool IsAggressive() => secondsInVoid >= secondsInVoidForAggression;

    public override void OnDestroy()
    {
        onDespawn?.Invoke();
    }

    private IEnumerator EatPlayer()
    {
        if (!Player.main.IsAlive()) yield break;
        
        playerBeingEaten = true;
        consumePlayerSfx.Play();
        Player.main.playerAnimator.SetTrigger("player_death_explosion");
        cinematicController.StartCinematicMode(Player.main);
        yield return new WaitForSeconds(1f);

        Player.main.liveMixin.Kill(DamageType.Electrical);
        cinematicController.EndCinematicMode();
        yield return new WaitForSeconds(5f);

        playerBeingEaten = false;
    }
}