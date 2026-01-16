using System.Collections;
using PrototypeSubMod.Facilities;
using PrototypeSubMod.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace PrototypeSubMod.Factors;

public class FactorUnlockTerminal : MonoBehaviour
{
    [SerializeField] private MultipurposeAlienTerminal unlockTerminal;
    [SerializeField] private Animator animator;
    [SerializeField] private DummyTechType unlockTechType;
    [SerializeField] private UnityEvent onInteracted;

    private void Start()
    {
        if (KnownTech.Contains(unlockTechType.TechType))
        {
            animator.SetTrigger("InstantActivate");
            unlockTerminal.ForceInteracted();
            return;
        }
        
        unlockTerminal.onTerminalInteracted += OnInteracted;
    }

    private void OnInteracted()
    {
        animator.SetBool("Activated", true);
        onInteracted?.Invoke();
    }

    public void OnActivationFinished()
    {
        var pdaLog = $"Proto{unlockTechType.TechType.ToString()}Unlock";
        if (!Language.main.Contains(pdaLog))
        {
            KnownTech.Add(unlockTechType.TechType);
            PDAEncyclopedia.Add(unlockTechType.TechType.ToString(), true);
            throw new System.Exception($"No language line for {pdaLog} detected!");
        }
        
        PDALog.Add(pdaLog);
        var data = Language.main.GetMetaData(pdaLog);
        float delay = 0;
        for (int i = 0; i < data.lineCount; i++)
        {
            delay += data.GetLine(i).duration;
        }

        StartCoroutine(UnlockFactorDelayed(delay));
    }

    private IEnumerator UnlockFactorDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        KnownTech.Add(unlockTechType.TechType);
        PDAEncyclopedia.Add(unlockTechType.TechType.ToString(), true);
    }
}