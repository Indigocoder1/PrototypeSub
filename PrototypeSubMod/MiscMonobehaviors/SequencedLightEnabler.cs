using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class SequencedLightEnabler : MonoBehaviour
{
    [SerializeField] private List<LightGroup> lightGroups;
    [SerializeField] private float initialDelay;
    [SerializeField] private float delayBetweenGroups;
    [SerializeField] private float activationDuration;
    [SerializeField] private AnimationCurve intensityOverDuration = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField, HideInInspector] public int[] lightSegmentLengths;
    [SerializeField, HideInInspector] public List<float> intensities;
    [SerializeField, HideInInspector] public List<Light> lights;

    private List<LightGroup> serializedLightGroups = new();
    
    private void OnValidate()
    {
        lightSegmentLengths = new int[lightGroups.Count];
        lights = new List<Light>();
        intensities = new List<float>();
        for (int i = 0; i < lightGroups.Count; i++)
        {
            lightSegmentLengths[i] = lightGroups[i].lightsInGroup.Length;
            lights.AddRange(lightGroups[i].lightsInGroup);
            foreach (var light in lightGroups[i].lightsInGroup)
            {
                intensities.Add(light.intensity);
            }
        }
    }

    private void Start()
    {
        int runningSum = 0;
        for (int i = 0; i < lightSegmentLengths.Length; i++)
        {
            var lights = this.lights.GetRange(runningSum, lightSegmentLengths[i]).ToArray();
            var intensities = this.intensities.GetRange(runningSum, lightSegmentLengths[i]).ToArray();
            var item = new LightGroup(lights, intensities);
            serializedLightGroups.Add(item);
            runningSum += lightSegmentLengths[i];
        }
        
        foreach (var group in serializedLightGroups)
        {
            foreach (var light in group.lightsInGroup)
            {
                light.intensity = intensityOverDuration.Evaluate(0);
            }
        }
    }

    public void ActivateLightsSequentially()
    {
        UWE.CoroutineHost.StartCoroutine(ActivateLights());
    }

    private IEnumerator ActivateLights()
    {
        yield return new WaitForSeconds(initialDelay);
        
        foreach (var lightGroup in serializedLightGroups)
        {
            StartCoroutine(ActivateLightGroup(lightGroup));
            yield return new WaitForSeconds(delayBetweenGroups);
        }
    }

    private IEnumerator ActivateLightGroup(LightGroup lightGroup)
    {
        float currentGroupActivation = 0;
        while (currentGroupActivation < activationDuration)
        {
            int index = 0;
            foreach (var light in lightGroup.lightsInGroup)
            {
                light.intensity = intensityOverDuration.Evaluate(currentGroupActivation / activationDuration) * lightGroup.intensities[index];
                index++;
            }
            currentGroupActivation += Time.deltaTime;
            yield return null;
        }
    }
}

[Serializable]
public class LightGroup
{
    public LightGroup(Light[] lightsInGroup, float[] intensities)
    {
        this.lightsInGroup = lightsInGroup;
        this.intensities = intensities;
    }

    public Light[] lightsInGroup;
    [HideInInspector] public float[] intensities;
}