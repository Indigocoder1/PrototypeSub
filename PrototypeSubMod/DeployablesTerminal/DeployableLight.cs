using System;
using System.Collections;
using Nautilus.Json;
using System.Net;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PrototypeSubMod.DeployablesTerminal;

internal class DeployableLight : MonoBehaviour, IProtoTreeEventListener
{
    [Header("Deployment")]
    [SerializeField] private PrefabIdentifier identifier;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Stabilizer stabilizer;
    [SerializeField] private float scaleSpeed;
    [SerializeField] private Light light;
    [SerializeField] private Collider[] undeployedColliders;
    [SerializeField] private Collider[] deployedColliders;
    [SerializeField] private float deployDelay;
    [SerializeField] private float lifetime;

    [Header("SFX")]
    [SerializeField] private FMOD_CustomEmitter deploySFX;
    [SerializeField] private FMOD_CustomLoopingEmitter loopingSFX;
    [SerializeField] private FMOD_CustomEmitter breakSFX;

    [Header("Breaking")]
    [SerializeField] private GameObject topHalf;
    [SerializeField] private GameObject bottomHalf;
    [SerializeField] private GameObject lightVisual;

    private float currentLifetime;
    private float lightRange;
    private Vector3 volumetricSize;
    private Pickupable pickupable;
    private EcoTarget ecoTarget;

    private bool piecesSeparated;
    private bool activated;

    private void Awake()
    {
        SetCollidersDeployed(false);
        lightRange = light.range;
        volumetricSize = light.transform.localScale;

        light.range = 0;
        light.transform.localScale = Vector3.zero;
        pickupable = GetComponent<Pickupable>();
        ecoTarget = GetComponent<EcoTarget>();
        ecoTarget.enabled = false;

        Plugin.GlobalSaveData.OnStartedSaving += SaveLifetimes;
    }

    private void SetCollidersDeployed(bool deployed)
    {
        foreach (var col in undeployedColliders)
        {
            col.enabled = !deployed;
        }
        
        foreach (var col in deployedColliders)
        {
            col.enabled = deployed;
        }
    }
    
    private void SetAllCollidersEnabled(bool enabled)
    {
        foreach (var col in undeployedColliders)
        {
            col.enabled = enabled;
        }
        
        foreach (var col in deployedColliders)
        {
            col.enabled = enabled;
        }
    }

    private void Start()
    {
        TryRestartLight();
        UWE.CoroutineHost.StartCoroutine(CancelDecoyDespawn());
    }

    private IEnumerator CancelDecoyDespawn()
    {
        yield return null;
        GetComponent<CyclopsDecoy>().CancelInvoke(nameof(CyclopsDecoy.Despawn));
    }

    public void LaunchWithForce(float force, Vector3 previousVelocity)
    {
        stabilizer.enabled = false;

        rb.AddForce((transform.forward * force) + previousVelocity, ForceMode.Impulse);

        SetAllCollidersEnabled(false);
        Invoke(nameof(ActivateLight), deployDelay);
        Invoke(nameof(ActivateColliders), deployDelay / 4f);
        Destroy(pickupable);
    }

    private void OnDrop()
    {
        Invoke(nameof(ActivateLight), deployDelay);
        Destroy(pickupable);
    }

    private void ActivateLight()
    {
        activated = true;
        ecoTarget.enabled = true;
        stabilizer.enabled = true;
        animator.SetTrigger("Activate");
        deploySFX.Play();
    }

    private void ActivateColliders()
    {
        SetCollidersDeployed(false);
    }

    private void Update()
    {
        if (currentLifetime < lifetime)
        {
            currentLifetime += Time.deltaTime;
        }
        else if (!piecesSeparated)
        {
            BreakLight();
        }
        
        if (!activated) return;

        float targetRange = activated && !piecesSeparated ? lightRange : 0;
        Vector3 targetScale = activated && !piecesSeparated ? volumetricSize : Vector3.zero;

        light.range = Mathf.Lerp(light.range, targetRange, Time.deltaTime * scaleSpeed);
        light.transform.localScale = Vector3.Lerp(light.transform.localScale, targetScale, Time.deltaTime * scaleSpeed);

        if (light.transform.localScale.magnitude > targetScale.magnitude - 0.5f && !piecesSeparated)
        {
            loopingSFX.Play();
        }

        if (light.range < 1e-5)
        {
            light.range = 0;
            light.enabled = false;
        }
        else
        {
            light.enabled = true;
        }
    }

    private void BreakLight()
    {
        piecesSeparated = true;

        topHalf.transform.SetParent(null);
        bottomHalf.transform.SetParent(null);
        lightVisual.SetActive(false);

        var rb1 = topHalf.AddComponent<Rigidbody>();
        var rb2 = bottomHalf.AddComponent<Rigidbody>();

        rb1.interpolation = RigidbodyInterpolation.Interpolate;
        rb2.interpolation = RigidbodyInterpolation.Interpolate;

        rb1.AddForce((Random.onUnitSphere * 5f) + (topHalf.transform.forward * 10f));
        rb2.AddForce((Random.onUnitSphere * 5f) - (bottomHalf.transform.forward * 10f));

        GetComponentInChildren<Animator>().enabled = false;

        Destroy(topHalf, 10f);
        Destroy(bottomHalf, 10f);
        Destroy(gameObject, 10f);

        breakSFX.Play();
        loopingSFX.Stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        ecoTarget.enabled = false;
    }

    public void OnHandHover(HandTargetEventData data)
    {
        HandReticle main = HandReticle.main;
        var useText = Language.main.GetFormat("DestroyDeployableLight", GameInput.FormatButton(GameInput.Button.LeftHand));
        main.SetTextRaw(HandReticle.TextType.Hand, useText);
    }

    public void OnHandUse(HandTargetEventData data)
    {
        BreakLight();
        currentLifetime = 0;
    }

    public void OnDeployAnimFinished()
    {
        SetCollidersDeployed(true);
    }

    public void OnProtoSerializeObjectTree(ProtobufSerializer serializer) { }

    public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
    {
        TryRestartLight();
    }

    private void TryRestartLight()
    {
        if (!identifier) return;
        
        if (Plugin.GlobalSaveData.deployableLightLifetimes.TryGetValue(identifier.Id, out currentLifetime) && currentLifetime < lifetime)
        {
            ActivateLight();
        }
        else if (currentLifetime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        Plugin.GlobalSaveData.OnStartedSaving -= SaveLifetimes;
        breakSFX.Stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        loopingSFX.Stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        if (!identifier) return;
        
        if (currentLifetime > 0)
        {
            SaveLifetimes(null, null);
        }
        else
        {
            Plugin.GlobalSaveData.deployableLightLifetimes.Remove(identifier.Id);
        }
    }

    private void SaveLifetimes(object sender, JsonFileEventArgs args)
    {
        if (piecesSeparated)
        {
            Destroy(gameObject);
            return;
        }

        if (Plugin.GlobalSaveData.deployableLightLifetimes.ContainsKey(identifier.Id))
        {
            Plugin.GlobalSaveData.deployableLightLifetimes[identifier.Id] = currentLifetime;
        }
        else
        {
            Plugin.GlobalSaveData.deployableLightLifetimes.Add(identifier.Id, currentLifetime);
        }
    }
}
