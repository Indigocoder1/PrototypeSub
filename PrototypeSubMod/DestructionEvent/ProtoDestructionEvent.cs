using System.Collections;
using PrototypeSubMod.VehicleAccess;
using SubLibrary.SubFire;
using UnityEngine;

namespace PrototypeSubMod.DestructionEvent;

internal class ProtoDestructionEvent : MonoBehaviour, IOnTakeDamage
{
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private LiveMixin mixin;
    [SerializeField] private CanvasGroup hudCanvasGroup;
    [SerializeField] private VoiceNotification reactorMeltdownOccurred;
    [SerializeField] private Animator hydrolockDoorsAnimator;
    [SerializeField] private GameObject radiationObject;
    [SerializeField] private float meltdownWarningDuration = 18f;
    [SerializeField] private float radiationGrowthSpeed = 1f;

    [Header("Sequences")]
    [SerializeField] private DestructionSequence internalSequence;
    [SerializeField] private DestructionSequence externalSequence;
    
    private RadiatePlayerInRange radiate;
    private float targetRadius;

    private void Start()
    {
        DevConsole.RegisterConsoleCommand(this, "destroyproto");
        Player.main.playerDeathEvent.AddHandler(this, OnPlayerDied);
        
        radiate = radiationObject.GetComponent<RadiatePlayerInRange>();
    }

    public IEnumerator OnDestroySub()
    {
        yield return new WaitForSeconds(meltdownWarningDuration);

        DestroySub();
    }

    public void OnTakeDamage(DamageInfo damageInfo)
    {
        if (mixin.health > 0) return;

        if (Plugin.GlobalSaveData.prototypeDestroyed) return;

        StartCoroutine(OnDestroySub());
    }

    private void DestroySub()
    {
        Plugin.GlobalSaveData.prototypeDestroyed = true;

        subRoot.voiceNotificationManager.PlayVoiceNotification(reactorMeltdownOccurred, false, true);
        hydrolockDoorsAnimator.SetBool("HydrolockEnabled", true);
        radiationObject.SetActive(true);
        
        targetRadius = radiate.radiateRadius;
        radiate.radiateRadius = 0f;
        UWE.CoroutineHost.StartCoroutine(GrowRadiationRange());
        
        CleanupSub();
        StartSequences();
        
        subRoot.GetComponent<ProtoSaveStateManager>().UpdateManagerStatus();
    }

    private IEnumerator GrowRadiationRange()
    {
        while (radiate.radiateRadius < targetRadius)
        {
            radiate.radiateRadius += radiationGrowthSpeed * Time.deltaTime;

            // Clamp to avoid overshooting
            if (radiate.radiateRadius > targetRadius)
                radiate.radiateRadius = targetRadius;

            yield return null; // wait one frame
        }
    }

    public void DestroySubNoSequence()
    {
        Plugin.GlobalSaveData.prototypeDestroyed = true;
        subRoot.GetComponent<ProtoSaveStateManager>().UpdateManagerStatus();
    }

    private void CleanupSub()
    {
        subRoot.subWarning = true;
        subRoot.fireSuppressionState = false;
        subRoot.silentRunning = false;
        
        var subFire = subRoot.GetComponentInChildren<ModdedSubFire>(true);
        subFire.CreateFire(subRoot.GetComponentInChildren<SubRoom>(true));
        subRoot.GetComponentInChildren<SubFloodAlarm>().NewAlarmState();
        
        foreach (var item in subRoot.GetComponentsInChildren<FMOD_CustomEmitter>(true))
        {
            item.Stop();
        }

        foreach (var damagePoint in subRoot.GetComponentsInChildren<CyclopsDamagePoint>(true))
        {
            damagePoint.OnRepair();
        }
        
        UWE.CoroutineHost.StartCoroutine(CleanupDelayed());
        subRoot.subDestroyed = true;
    }

    private IEnumerator CleanupDelayed()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        hudCanvasGroup.alpha = 0;
    }
    
    private void StartSequences()
    {
        if (Player.main.currentSub == subRoot)
        {
            internalSequence.StartSequence(subRoot);
        }
        else
        {
            externalSequence.StartSequence(subRoot);
        }
    }

    private void OnConsoleCommand_destroyproto(NotificationCenter.Notification n)
    {
        DestroySub();
    }

    private void OnPlayerDied(Player player)
    {
        if (!Plugin.GlobalSaveData.prototypeDestroyed) return;

        foreach (var room in subRoot.GetComponentsInChildren<SubRoom>(true))
        {
            var nodes = room.GetSpawnNodes();
            foreach (var node in nodes)
            {
                for (int i = 0; i < node.childCount; i++)
                {
                    Destroy(node.GetChild(i).gameObject);
                }
            }
        }
        
        subRoot.subWarning = false;
        subRoot.fireSuppressionState = false;
        subRoot.silentRunning = false;
        subRoot.BroadcastMessage("NewAlarmState");
    }
}
