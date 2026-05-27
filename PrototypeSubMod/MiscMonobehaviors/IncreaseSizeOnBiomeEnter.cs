using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class IncreaseSizeOnBiomeEnter : MonoBehaviour, IScheduledUpdateBehaviour
{
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
        }
        else if (hasScaledUp && Player.main.biomeString != biome)
        {
            transform.localScale *= (1 / scaleFactor);
            hasScaledUp = false;
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