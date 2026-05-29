using System.Collections;
using SubLibrary.Monobehaviors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace PrototypeSubMod.MiscMonobehaviors.Emission;

internal class EmissionColorController : PrefabModifier
{
    [SerializeField] private GameObject[] objectRoots;
    [SerializeField] private float transitionSpeed;
    [SerializeField] private bool updateOnStart = true;

    private Dictionary<Material, Color> trackedMaterials = new();
    private Dictionary<Component, EmissionRegistrarData> overrideColorData = new();
    private bool initialized;

    private Color tempColor;
    private bool tempColorActive;
    private float transitionTimeOut;
    private float currentTransitionTime;

    private void Awake()
    {
        UWE.CoroutineHost.StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        if (initialized) yield break;

        yield return new WaitUntil(() => objectRoots.All(o => o.activeInHierarchy));
        yield return null;
        
        transitionTimeOut = 50f / transitionSpeed;
        if (!updateOnStart)
        {
            currentTransitionTime = transitionTimeOut;
        }

        foreach (var objectRoot in objectRoots)
        {
            RegisterMaterials(objectRoot);
        }

        initialized = true;
    }

    private void RegisterMaterials(GameObject objectRoot)
    {
        foreach (var rend in objectRoot.GetComponentsInChildren<Renderer>(true))
        {
            if (rend.GetComponentInParent<EmissionControllerExempt>()) continue;
            
            if (rend.GetComponentInParent<Constructable>()) continue;

            foreach (var mat in rend.materials)
            {
                if (!mat.IsKeywordEnabled("MARMO_EMISSION")) continue;

                trackedMaterials.Add(mat, mat.GetColor("_GlowColor"));
            }
        }
    }

    private void Update()
    {
        if (!initialized) return;

        if (currentTransitionTime < transitionTimeOut)
        {
            currentTransitionTime += Time.deltaTime;
        }
        else
        {
            return;
        }

        currentTransitionTime = Mathf.Clamp(currentTransitionTime, 0, transitionTimeOut);
        foreach (var material in trackedMaterials.Keys)
        {
            Color targetCol = tempColorActive ? tempColor : trackedMaterials[material];
            Color currentCol = Color.Lerp(material.GetColor("_GlowColor"), targetCol, currentTransitionTime / transitionTimeOut);
            material.SetColor("_GlowColor", currentCol);
        }
    }

    public void RegisterTempColor(Component component, EmissionRegistrarData registerData)
    {
        overrideColorData[component] = registerData;
        tempColorActive = true;
        currentTransitionTime = 0;

        UpdateTempColor();
    }

    public void RemoveTempColor(Component component)
    {
        if (!overrideColorData.ContainsKey(component)) return;
        
        overrideColorData.Remove(component);
        tempColorActive = overrideColorData.Count > 0;
        currentTransitionTime = 0;

        UpdateTempColor();
    }

    private void UpdateTempColor()
    {
        int greatestPriority = int.MinValue;
        foreach (var data in overrideColorData.Values)
        {
            if (data.priority > greatestPriority)
            {
                greatestPriority = data.priority;
                tempColor = data.overrideColor;
            }
        }
    }
    
    public void ForceUpdate()
    {
        currentTransitionTime = 0;
    }

    public struct EmissionRegistrarData
    {
        public Color overrideColor;
        public int priority;

        public EmissionRegistrarData(Color overrideColor, int priority = 10)
        {
            this.overrideColor = overrideColor;
            this.priority = priority;
        }
    }
}
