using PrototypeSubMod.Registration;
using UnityEngine;

namespace PrototypeSubMod.PrototypeStory;

public class TransmissionRunupMusic : MonoBehaviour, IScheduledUpdateBehaviour
{
    [SerializeField] private FMOD_CustomEmitter musicPlayer;
    [SerializeField] private float minTimeInRunupToPlay;

    private float timeEnteredRunup;
    private bool wasInRunup;
    private bool hasPlayed;

    public string GetProfileTag() => "TransmissionRunupMusic";
    
    public void ScheduledUpdate()
    {
        var inRunup = Player.main.biomeString == BiomeRegisterer.TransmissionRunupBiome;

        if (inRunup && !wasInRunup)
        {
            timeEnteredRunup = Time.time;
        }
        else if (inRunup && Time.time >= timeEnteredRunup + minTimeInRunupToPlay && !hasPlayed)
        {
            musicPlayer.Play();
            hasPlayed = true;
        }

        if (!inRunup && wasInRunup)
        {
            musicPlayer.Stop();
            timeEnteredRunup = float.MaxValue;
            hasPlayed = false;
        }

        wasInRunup = inRunup;
    }
    
    public void OnEnable()
    {
        UpdateSchedulerUtils.Register(this);
    }

    public void OnDisable()
    {
        UpdateSchedulerUtils.Deregister(this);
    }

    public int scheduledUpdateIndex { get; set; }
}