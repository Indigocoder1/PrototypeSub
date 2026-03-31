using Nautilus.Utility;
using PrototypeSubMod.LightDistortionField;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
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
    [SerializeField] private float timeBetweenTargetJitters = 0.2f;
    [SerializeField] private float jitterMagnitude = 5f;
    
    [Header("SFX")]
    [SerializeField] private WyrmRoarManager roarManager;
    [SerializeField] private FMOD_CustomEmitter shotChargeSfx;
    [SerializeField] private FMOD_CustomEmitter shotTravelSfx;
    [SerializeField] private FMOD_CustomEmitter shotHitSfx;
    [SerializeField] private FMOD_CustomEmitter shotReflectSfx;
    [SerializeField] private FMOD_CustomEmitter reflectShutdownSfx;
    [SerializeField] private FMOD_CustomEmitter shotChargeStartSfx;
    
    private GameObject laserVFX;
    private GameObject muzzleVFX;
    private GameObject impactVFX;
    private Vector3 lastJitterVector;
    private bool performing;
    private bool canShoot;
    private bool hasShot;
    private float currentChargeUpTime;
    private float timeLastJittered;
    private int rightHandVectorSign;
    private int attackStage;
    private int timesParried;
    private bool aimStarted;

    private void Start()
    {
        targetingLineRenderer.enabled = false;
        muzzleVFX = Instantiate(VFXSunbeam.main.muzzlePrefab.transform.Find("xBeam").gameObject);
        laserVFX = Instantiate(VFXSunbeam.main.beamPrefab);

        StartCoroutine(SetImpactVFX());

        muzzleVFX.SetActive(false);
        laserVFX.SetActive(false);

        muzzleVFX.transform.localScale = Vector3.one * 0.25f;
        Destroy(muzzleVFX.GetComponent<VFXDestroyAfterSeconds>());
    }

    private IEnumerator SetImpactVFX()
    {
        var task = CraftData.GetPrefabForTechTypeAsync(TechType.PrecursorDroid);
        yield return task;
        var droid = task.GetResult();
        var livemixin = droid.GetComponent<LiveMixin>();
        impactVFX = Instantiate(livemixin.deathEffect);

        impactVFX.SetActive(false);
    }

    public override float Evaluate(Creature creature, float time)
    {
        return performing ? 1 : Random.Range(0f, 0.8f);
    }
    
    public override void Perform(Creature creature, float time, float deltaTime)
    {
        if (performing) return;
        
        base.Perform(creature, time, deltaTime);
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
    
    public void OverrideStopPerform()
    {
        performing = false;
    }
    
    private void Update()
    {
        if (!performing) return;
        
        if (currentChargeUpTime > 0)
        {
            if (!aimStarted)
            {
                shotChargeSfx.Play();
                shotChargeStartSfx.Play();
                aimStarted = true;
            }
            currentChargeUpTime -= Time.deltaTime;
            HandleTargetingLaser();
        }
        else if (canShoot && !hasShot)
        {
            StartCoroutine(Shoot());
            aimStarted = false;
        }
        
        var angle = Mathf.Abs(
            Vector3.Angle(GetTargetMixin().transform.position - transform.position, transform.forward));
        const float angleToChargeLaser = 30f;
        if (angleToChargeLaser < angle && !canShoot && attackStage == 2)
        {
            canShoot = true;
        }
        
        if (attackStage == 2 && !hasShot && canShoot && !targetingLineRenderer.enabled)
        {
            currentChargeUpTime = chargeUpTime;
            FMODUWE.PlayOneShot(shotChargeSfx.asset, transform.position);
            targetingLineRenderer.enabled = true;
        }
    }

    public void OnShotParried(Vector3 returnFrom)
    {
        StartCoroutine(ReturnParryProjectile(returnFrom));
    }

    private IEnumerator ReturnParryProjectile(Vector3 returnFrom)
    {
        yield return new WaitForEndOfFrame();
        shotReflectSfx.Play();
        
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
        roarManager.PlayRoar(Player.main.transform.position);

        if (timesParried < parriesToResetAggression) return;
        
        GetComponent<ProtoAggressiveWorm>().ResetAggression(timePassiveAfterParries);
        reflectShutdownSfx.Play();
        timesParried = 0;
    }

    private void OnReachedTarget()
    {
        attackStage++;
        if (attackStage > GetAttackPoints().Length - 1)
        {
            performing = false;
        }
        
        if (attackStage > GetAttackPoints().Length - 1) return;
        
        wormAnimator.SetTravelTarget(GetAttackPoints()[attackStage], OnReachedTarget);
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

        if (targetMixin == null)
        {
            yield break;
        }

        var targetPos = targetMixin.transform.position;
        var laserTargetPoint = targetPos;
        var effectHandler = targetMixin.GetComponentInChildren<CloakEffectHandler>();

        if (effectHandler != null)
        {
            laserTargetPoint = effectHandler.GetActive()
                ? effectHandler.GetClosestPointOnSurface(
                    targetPos + (targetMixin.transform.forward + targetMixin.transform.up) * 50f, 5f)
                : effectHandler.GetClosestPointOnSurface(
                    targetPos + targetMixin.transform.forward * 50f, -15f);
        }


        var beamMaterials = laserVFX.GetComponent<Renderer>().materials;
        var originalPoint = laserOrigin.position;
        shotTravelSfx.Play();
        shotChargeSfx.Stop();

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

        laserVFX.SetActive(false);
        muzzleVFX.SetActive(false);

        var mixin = GetAttackMixin(laserTargetPoint);
        if (mixin == null) yield break;

        DamageTarget(laserTargetPoint, mixin);

        shotHitSfx.Play();
        roarManager.PlayRoar(Player.main.transform.position);
        MainCameraControl.main.ShakeCamera(5, -1, MainCameraControl.ShakeMode.Linear, 1);

        // Do impact VFX if hitting a sub
        if (mixin.GetComponent<SubRoot>() != null) StartCoroutine(DoImpactVFX(laserTargetPoint));
    }

    private IEnumerator DoImpactVFX(Vector3 position)
    {
        var task = CraftData.GetPrefabForTechTypeAsync(TechType.PrecursorDroid);
        yield return task;
        var droid = task.GetResult();
        var livemixin = droid.GetComponent<LiveMixin>();
        var impactVFXInstance = Instantiate(livemixin.deathEffect);

        impactVFXInstance.transform.localScale = Vector3.one * 15f;
        var offsetScale = 10f;
        var randomOffset = new Vector3(Random.Range(-offsetScale, offsetScale), Random.Range(-offsetScale, offsetScale), Random.Range(-offsetScale, offsetScale));
        impactVFXInstance.transform.position = position + randomOffset;
        impactVFXInstance.SetActive(true);

        foreach (var particleSystem in impactVFXInstance.GetComponentsInChildren<ParticleSystem>())
        {
            particleSystem.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
    }

    private LiveMixin GetAttackMixin(Vector3 laserTargetPoint)
    {
        var colliders = Physics.OverlapSphere(laserTargetPoint, 10f);
        LiveMixin mixin = null;
        foreach (var collider in colliders)
        {
            if (collider.attachedRigidbody == null) continue;
            
            if (collider.attachedRigidbody.TryGetComponent(out mixin)) break;
        }

        return mixin;
    }

    private void DamageTarget(Vector3 laserTargetPoint, LiveMixin hitMixin)
    {
        hitMixin.TakeDamage(attackDamage, laserTargetPoint, DamageType.Electrical, gameObject);
    }

    private void HandleTargetingLaser()
    {
        var targetMixin = GetTargetMixin();
        if (targetMixin == null) return;

        var targetPos = targetMixin.transform.position;

        var positions = new Vector3[2];
        positions[0] = laserOrigin.position;

        var cloak = targetMixin.GetComponentInChildren<CloakEffectHandler>();

        if (cloak != null && cloak.GetActive())
        {
            // Aim at cloaked sub
            positions[1] = cloak.GetContinuousPointOnSurface() + lastJitterVector;
        }
        else if (cloak != null)
        {
            // Aim at sub with offset when cloak isn't active
            positions[1] = cloak.GetClosestPointOnSurface(
                targetPos + targetMixin.transform.forward * 50f, -4f) + lastJitterVector;
        }
        else if (Player.main.currentSub != null)
        {
            var currentSub = Player.main.currentSub;
            positions[1] = currentSub.centerOfMass.position + currentSub.subAxis.forward * 30f + lastJitterVector;
        }
        else
        {
            // Aim at player with slight downward offset to avoid camera clipping
            positions[1] = targetPos + new Vector3(0, -3, 0);
        }

        if (Time.time >= timeLastJittered + timeBetweenTargetJitters)
        {
            timeLastJittered = Time.time;
            lastJitterVector = Random.onUnitSphere * jitterMagnitude;
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
        var sign = Mathf.Sign(Random.Range(-1f, 1f));
        sign = sign == 0 ? 1 : sign;
        var rightDir = Vector3.Cross(forwardDir, Vector3.up) * sign;
        // Offset to the right to set up for the swing towards the target
        points[0] = targetCenter + (forwardDir + rightDir) * setupDist;
        // Go off towards the right
        points[1] = targetCenter + (forwardDir + rightDir * rightHandVectorSign) * setupDist;
        // Straight towards target
        points[2] = targetCenter + forwardDir * 20f;

        return points;
    }

    private LiveMixin GetTargetMixin()
    {
        var player = Player.main;
        if (player.currentSub) return player.currentSub.live;
        if (player.lastValidSub &&
            Vector3.Distance(player.lastValidSub.transform.position, player.transform.position) < 20f)
        {
            return player.lastValidSub.live;
        }

        return player.liveMixin;
    }

    public override bool NeedsToBeChecked(float time) => true;

    private void OnDestroy()
    {
        Destroy(laserVFX);
        Destroy(muzzleVFX);
    }
}