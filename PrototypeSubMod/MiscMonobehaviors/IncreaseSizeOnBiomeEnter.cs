using System;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class IncreaseSizeOnBiomeEnter : MonoBehaviour, IScheduledUpdateBehaviour
{
    public static event Action<(string biome, bool sizeIncreased)> OnBiomeSizeChanged;
    
    [SerializeField] public string biome;
    [SerializeField] public float scaleFactor;

    private bool hasScaledUp;
    
    public void SetInfo(string biome, float scaleFactor)
    {
        this.biome = biome;
        this.scaleFactor = scaleFactor;
    }

    public string GetProfileTag() => "IncreaseSizeOnBiomeEnter";
    public void ScheduledUpdate()
    {
        if (!hasScaledUp && Player.main.biomeString == biome)
        {
            transform.localScale *= scaleFactor;
            hasScaledUp = true;
            OnBiomeSizeChanged?.Invoke((biome, hasScaledUp));
        }
        else if (hasScaledUp && Player.main.biomeString != biome)
        {
            transform.localScale *= (1 / scaleFactor);
            hasScaledUp = false;
            OnBiomeSizeChanged?.Invoke((biome, hasScaledUp));
        }
    }

    public int scheduledUpdateIndex { get; set; }

    private void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }

    private void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }
}