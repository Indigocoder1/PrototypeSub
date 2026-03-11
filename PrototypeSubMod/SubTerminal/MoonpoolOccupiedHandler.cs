using System;
using PrototypeSubMod.LightDistortionField;
using UnityEngine;

namespace PrototypeSubMod.SubTerminal;

internal class MoonpoolOccupiedHandler : MonoBehaviour
{
    public static readonly Bounds MoonpoolBounds = new Bounds(new Vector3(465.67f, -109.81f, 1216.69f), new Vector3(53.32f, 34.50f, 89.36f));

    public event Action onHasSubChanged;
    
    public bool MoonpoolHasSub
    {
        get;
        private set;
    }

    public GameObject SubInMoonpool { get; private set; }
    
    [SerializeField] private BoxCollider moonpoolBounds;

    private void Start()
    {
        Initialize();
    }
    
    public void CheckForSub()
    {
        bool foundSub = false;
        SubInMoonpool = null;

        foreach (var handler in CloakEffectHandler.EffectHandlers)
        {
            var subRoot = handler.GetComponentInParent<SubRoot>();
            if (moonpoolBounds.bounds.Contains(subRoot.transform.position))
            {
                foundSub = true;
                SubInMoonpool = subRoot.gameObject;
                break;
            }
        }

        if (foundSub != MoonpoolHasSub)
        {
            MoonpoolHasSub = foundSub;
            onHasSubChanged?.Invoke();
        }

        MoonpoolHasSub = foundSub;
    }

    private void Initialize()
    {
        CancelInvoke(nameof(CheckForSub));
        InvokeRepeating(nameof(CheckForSub), 0, 5f);
    }

    public Bounds GetBounds() => moonpoolBounds.bounds;
    public Transform GetTransform() => moonpoolBounds.transform;
}
