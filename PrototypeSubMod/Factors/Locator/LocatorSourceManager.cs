using System;
using System.Linq;
using Nautilus.Handlers;
using PrototypeSubMod.Prefabs.Factors;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Factors.Locator;

public class LocatorSourceManager : MonoBehaviour
{
    private const float distanceUnit = 200f;
    private float deltaDistance = 10f;
    private float timePingStart;
    private float timeLoopStart;
    private (string name, Vector3 delta, float normDist) nearestFacility;
    private float relPitch;
    private float actualDuration;

    private bool inUse = false;
    public bool stopping = false;

    [SerializeField] private FMOD_CustomEmitter pingSfx;
    public Locator locatorFactor;


    public void StartUse()
    {
        inUse = true;
        var loc = Player.main.transform.position;
        nearestFacility = Plugin.FACILITY_POSITIONS
            .Select(kv => (kv.Key, kv.Value - loc, dist: Vector3.Distance(loc, kv.Value) / distanceUnit))
            .OrderBy(t => t.dist)
            .FirstOrFallback(("Player", loc, 0f));
        if (nearestFacility.normDist == 0f) return;
        transform.localPosition = nearestFacility.delta.normalized * deltaDistance;
        timePingStart = Time.time;
        timeLoopStart = Time.time;
        float loops = 5f - nearestFacility.normDist;
        relPitch = Mathf.Pow(2f, loops);
        

        actualDuration = locatorFactor.duration / relPitch * loops;
        pingSfx.Play();
        HandlePingEffect();
    }

    private void Update()
    {
        if (inUse)
        {
            // continue to at least `actualDuration` and don't cut off mid-loop
            if (Time.time - timePingStart <= actualDuration || Time.time - timeLoopStart <= locatorFactor.duration / relPitch) 
            {
                HandlePingEffect();
                if (Time.time - timeLoopStart >= locatorFactor.duration / relPitch)
                {
                    if (stopping) 
                    {
                        Stop();
                        return;
                    }
                    timeLoopStart = Time.time;
                    pingSfx.Play();
                }
            }
            else
                Stop();
        }
        else
            transform.localPosition = Vector3.zero; // track player when not in use
    }

    public void GracefulStop()
    {
        stopping = true;
    }

    public void Stop()
    {
        inUse = false;
        stopping = false;
        pingSfx.Stop();
        transform.localPosition = Vector3.zero;
    }

    private void HandlePingEffect()
    {
        if (!CustomSoundHandler.TryGetCustomSoundChannel(pingSfx.GetInstanceID(), out var loopingChannel)) return;
        Debug.Log($"Successfully got locator sound channel.");
        loopingChannel.setPitch(relPitch);
    }

    // private void HandleSourceMotion()
    // {
    //     transform.position += nearestFacility.delta.normalized * deltaDistance * Time.deltaTime;
    // }
}