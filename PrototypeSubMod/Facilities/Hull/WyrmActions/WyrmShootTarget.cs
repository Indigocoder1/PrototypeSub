using System;
using System.Collections;
using PrototypeSubMod.LightDistortionField;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.Facilities.Hull.WyrmActions;

public class WyrmShootTarget : CreatureAction
{
    [SerializeField] private AggressiveWormAnimator wormAnimator;
    [SerializeField] private LineRenderer targetingLineRenderer;
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private AnimationCurve beamLengthCurve;
    [SerializeField] private AnimationCurve beamWidthCurve;
    [SerializeField] private float attackDamage;
    [SerializeField] private float chargeUpTime;
    [SerializeField] private float laserTravelTime;
    [SerializeField] private int parriesToResetAggression = 3;
    [SerializeField] private float timePassiveAfterParries;

    private CloakEffectHandler targetCloakHandler;
    private GameObject laserVFX;
    private GameObject muzzleVFX;
    private bool performing;
    private bool canShoot;
    private bool hasShot;
    private float currentChargeUpTime;
    private int prevChargeUpTime;
    private int rightHandVectorSign;
    private int attackStage;
    private int timesParried;

    private void Start()
    {
        targetingLineRenderer.enabled = false;
        muzzleVFX = Instantiate(VFXSunbeam.main.muzzlePrefab.transform.Find("xBeam").gameObject);
        laserVFX = Instantiate(VFXSunbeam.main.beamPrefab);
        muzzleVFX.SetActive(false);
        laserVFX.SetActive(false);

        muzzleVFX.transform.localScale = Vector3.one * 0.25f;
        Destroy(muzzleVFX.GetComponent<VFXDestroyAfterSeconds>());
    }

    public override float Evaluate(Creature creature, float time)
    {
        return performing ? 1 : Random.Range(0f, 0.8f);
    }
    
    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;
        
        base.Perform(creature, time, deltaTime);
        targetCloakHandler = GetTargetMixin().GetComponentInChildren<CloakEffectHandler>(true);
        performing = true;
        canShoot = false;
        hasShot = false;
        targetingLineRenderer.enabled = false;
        rightHandVectorSign = (int)Mathf.Sign(Random.Range(-1f, 1f));
        attackStage = 0;
        currentChargeUpTime = 0;
        wormAnimator.SetTravelTarget(GetAttackPoints()[attackStage], OnReachedTarget);
        Plugin.Logger.LogInfo($"Started shoot target");
    }
    
    public override void StopPerform(Creature creature, float time)
    {
        base.StopPerform(creature, time);
        performing = false;
    }
    
    private void Update()
    {
        if (!performing) return;
        
        wormAnimator.SetTravelTarget(GetAttackPoints()[attackStage], OnReachedTarget);
        if (currentChargeUpTime > 0)
        {
            currentChargeUpTime -= Time.deltaTime;
            HandleTargetingLaser();
        }
        else if (canShoot && !hasShot)
        {
            StartCoroutine(Shoot());
        }

        var angle = Mathf.Abs(
            Vector3.Angle(GetTargetMixin().transform.position - transform.position, transform.forward));
        const float angleToChargeLaser = 30f;
        if (attackStage == 2 && angle < angleToChargeLaser && !canShoot && !hasShot)
        {
            currentChargeUpTime = chargeUpTime;
            canShoot = true;
            targetingLineRenderer.enabled = true;
        }

        if (prevChargeUpTime != (int)currentChargeUpTime)
        {
            ErrorMessage.AddError($"Shooting in {(int)currentChargeUpTime + 1}");
        }

        prevChargeUpTime = (int)currentChargeUpTime;
    }

    public void OnShotParried(Vector3 returnFrom)
    {
        ErrorMessage.AddError("Parried!");

        StartCoroutine(ReturnParryProjectile(returnFrom));
    }

    private IEnumerator ReturnParryProjectile(Vector3 returnFrom)
    {
        yield return new WaitForEndOfFrame();
        laserVFX.SetActive(true);
        muzzleVFX.SetActive(true);
        var originalPosition = transform.position;
        var beamMaterials = laserVFX.GetComponent<Renderer>().materials;

        var ps = muzzleVFX.GetComponent<ParticleSystem>();
        ps.Stop();
        var main = ps.main;
        main.startDelay = new ParticleSystem.MinMaxCurve(0);
        main.startDelayMultiplier = 0;
        main.duration = laserTravelTime + 1f;
        ps.Play();
        laserVFX.transform.position = returnFrom;
        laserVFX.transform.LookAt(originalPosition);
        float travelTime = 0;
        while (travelTime < laserTravelTime)
        {
            float normalizedTravelTime = travelTime / laserTravelTime;
            var point = Vector3.Lerp(returnFrom, originalPosition, normalizedTravelTime);
            laserVFX.transform.position = point;
            laserVFX.transform.LookAt(originalPosition);
            
            var distance = Vector3.Distance(laserOrigin.position, point);
            var width = beamWidthCurve.Evaluate(normalizedTravelTime);
            var scale = new Vector3(width * 2f, width * 2f, distance);
            laserVFX.transform.localScale = scale;
            
            muzzleVFX.transform.position = point;
            var right = Vector3.Cross(Vector3.up, originalPosition - point);
            var up = Vector3.Cross(right, originalPosition - point);
            muzzleVFX.transform.rotation = Quaternion.LookRotation(right, up);

            var texOffset = new Vector2(beamLengthCurve.Evaluate(normalizedTravelTime), 0.5f);
            beamMaterials[0].SetTextureOffset(ShaderPropertyID._MainTex2, texOffset);
            
            travelTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        IncrementParry();
        laserVFX.SetActive(false);
        muzzleVFX.SetActive(false);
    }

    private void IncrementParry()
    {
        timesParried++;

        if (timesParried < parriesToResetAggression) return;
        
        ErrorMessage.AddError($"Resetting aggression for {timePassiveAfterParries} seconds");
        GetComponent<ProtoAggressiveWorm>().ResetAggression(timePassiveAfterParries);
    }

    private void OnReachedTarget()
    {
        attackStage++;
        if (attackStage > GetAttackPoints().Length - 1)
        {
            performing = false;
        }
    }

    private IEnumerator Shoot()
    {
        canShoot = false;
        hasShot = true;
        targetingLineRenderer.enabled = false;
        laserVFX.SetActive(true);
        muzzleVFX.SetActive(true);
        var ps = muzzleVFX.GetComponent<ParticleSystem>();
        ps.Stop();
        var main = ps.main;
        main.startDelay = new ParticleSystem.MinMaxCurve(0);
        main.startDelayMultiplier = 0;
        main.duration = laserTravelTime + 1f;
        ps.Play();
        var targetMixin = GetTargetMixin();
        var effectHandler = targetMixin.GetComponentInChildren<CloakEffectHandler>();
        var targetPos = targetMixin.transform.position;
        var laserTargetPoint = effectHandler.GetActive()
            ? effectHandler.GetClosestPointOnSurface(targetPos +
                                                     (targetMixin.transform.forward + targetMixin.transform.up) * 50f, 5f)
            : effectHandler.GetClosestPointOnSurface(targetPos + targetMixin.transform.forward * 50f, -15f);
        
        var beamMaterials = laserVFX.GetComponent<Renderer>().materials;
        var originalPoint = laserOrigin.position;
        
        ErrorMessage.AddError("Laser fired");
        
        float travelTime = 0;
        while (travelTime < laserTravelTime)
        {
            float normalizedTravelTime = travelTime / laserTravelTime;
            var point = Vector3.Lerp(originalPoint, laserTargetPoint, normalizedTravelTime);
            laserVFX.transform.position = laserOrigin.position;
            laserVFX.transform.LookAt(laserTargetPoint);
            
            var distance = Vector3.Distance(laserOrigin.position, point);
            var width = beamWidthCurve.Evaluate(normalizedTravelTime);
            var scale = new Vector3(width * 8f, width * 5f, distance);
            laserVFX.transform.localScale = scale;

            muzzleVFX.transform.position = point;
            var right = Vector3.Cross(Vector3.up, originalPoint - point);
            var up = Vector3.Cross(right, originalPoint - point);
            muzzleVFX.transform.rotation = Quaternion.LookRotation(-right, up);

            var texOffset = new Vector2(beamLengthCurve.Evaluate(normalizedTravelTime), 0.5f);
            beamMaterials[0].SetTextureOffset(ShaderPropertyID._MainTex2, texOffset);
            
            travelTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        DamageTarget(laserTargetPoint);
        laserVFX.SetActive(false);
        muzzleVFX.SetActive(false);
    }

    private void DamageTarget(Vector3 laserTargetPoint)
    {
        ErrorMessage.AddError("Laser reached target");

        var colliders = Physics.OverlapSphere(laserTargetPoint, 25f);
        bool hitTarget = false;
        foreach (var collider in colliders)
        {
            if (collider.attachedRigidbody == null) continue;
            
            if (!collider.attachedRigidbody.TryGetComponent(out LiveMixin mixin)) continue;

            mixin.TakeDamage(attackDamage, laserTargetPoint, DamageType.LaserCutter, gameObject);
            hitTarget = true;
            break;
        }

        ErrorMessage.AddError(hitTarget ? "Hit object" : "Missed object");
    }

    private void HandleTargetingLaser()
    {
        var targetMixin = GetTargetMixin();
        var targetPos = targetMixin.transform.position;
        var positions = new Vector3[2];
        positions[0] = laserOrigin.position;
        if (targetCloakHandler && targetCloakHandler.GetActive())
        {
            positions[1] = targetCloakHandler.GetContinuousPointOnSurface();
        }
        else if (targetCloakHandler)
        {
            positions[1] = targetCloakHandler.GetClosestPointOnSurface(targetPos + targetMixin.transform.forward * 50f, -4f);
        }
        else
        {
            positions[1] = targetPos;
        }
        
        targetingLineRenderer.SetPositions(positions);
    }
    
    private Vector3[] GetAttackPoints()
    {
        const float setupDist = 200;
        
        var points = new Vector3[3];
        var player = Player.main;
        Vector3 targetCenter;
        if (player.currentSub == null)
        {
            targetCenter = player.transform.position;
        }
        else
        {
            targetCenter = player.currentSub.centerOfMass.position;
        }
        
        var forwardDir = targetCenter.normalized;
        var rightDir = -Vector3.Cross(forwardDir, Vector3.up);
        // Offset to the right to set up for the swing towards the target
        points[0] = targetCenter + (forwardDir + rightDir) * setupDist;
        // Go off towards the right
        points[1] = targetCenter + (forwardDir + rightDir * rightHandVectorSign) * setupDist;
        // Straight towards target
        points[2] = targetCenter + Vector3.down * 20f;

        return points;
    }

    private LiveMixin GetTargetMixin()
    {
        var player = Player.main;
        if (player.currentSub) return player.currentSub.live;
        if (player.lastValidSub &&
            Vector3.Distance(player.lastValidSub.transform.position, player.transform.position) < 50f)
        {
            return player.lastValidSub.live;
        }

        return player.liveMixin;
    }

    public override bool NeedsToBeChecked(float time) => true;
}