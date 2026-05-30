using UnityEngine;
using UnityEngine.Events;

namespace PrototypeSubMod.UI.AbilitySelection;

internal class GenericRadialAbility : MonoBehaviour, IAbilityIcon
{
    [SerializeField] private bool showAbility = true;
    [SerializeField] private bool allowActivationWhenActive = true;
    [SerializeField] private Sprite sprite;
    [Tooltip("This object should be set inactive when the upgrade is inactive, and vice versa")]
    [SerializeField] private GameObject upgradeActiveObject;
    [SerializeField] private UnityEvent onActivated;
    [SerializeField] private UnityEvent onUnselected;

    public bool GetShouldShow() => showAbility && !forceDisabled;
    public bool GetIsInstalled() => true;
    public void ForceDisabled()
    {
        forceDisabled = true;
        OnSelectedChanged(false);
    }

    public Sprite GetSprite() => sprite;
    public TechType GetTechType() => TechType.None;
    public bool GetCanActivate() => !forceDisabled;

    private bool activationFailure;
    private bool forceDisabled;
    
    public bool OnActivated()
    {
        if (forceDisabled) return false;
        
        if (GetActive() && !allowActivationWhenActive) return false;

        onActivated?.Invoke();
        if (activationFailure)
        {
            activationFailure = false;
            return false;
        }
        return true;
    }

    public void OnSelectedChanged(bool changed)
    {
        if (!changed)
        {
            onUnselected?.Invoke();
        }
    }

    public bool GetActive()
    {
        if (!upgradeActiveObject) return false;

        return upgradeActiveObject.activeSelf;
    }

    public void SetQueuedActivationFailure()
    {
        activationFailure = true;
    }
}
