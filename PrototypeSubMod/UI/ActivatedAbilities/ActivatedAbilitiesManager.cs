using PrototypeSubMod.UI.AbilitySelection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrototypeSubMod.UI.ActivatedAbilities;

internal class ActivatedAbilitiesManager : MonoBehaviour
{
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private Transform[] abilityIconSlots;
    [SerializeField] private SelectionMenuManager menuManager;
    [SerializeField] private TetherManager tetherManager;

    private Dictionary<IAbilityIcon, ActiveAbilityIcon> activeAbilities = new();
    private int installedUpgradeCount;
    
    private void Start()
    {
        tetherManager.onAbilityActivatedChanged += OnAbilityActivatedChanged;
    }

    public void OnAbilityActivatedChanged(IAbilityIcon icon)
    {
        if (icon == null) return;
        
        if (icon.GetActive() && !activeAbilities.ContainsKey(icon))
        {
            var newIcon = CreateNewIcon(icon);
            activeAbilities.Add(icon, newIcon);
            installedUpgradeCount++;
        }
        else if (!icon.GetActive() && activeAbilities.TryGetValue(icon, out var ability))
        {
            activeAbilities.Remove(icon);
            Destroy(ability.gameObject);
            installedUpgradeCount--;

            int index = 0;
            foreach (var activeAbility in activeAbilities.Values)
            {
                activeAbility.transform.SetParent(abilityIconSlots[index]);
                activeAbility.transform.localPosition = Vector3.zero;
                index++;
            }
        }
    }
    
    public int GetActiveAbilityCount() => installedUpgradeCount;
    
    private ActiveAbilityIcon CreateNewIcon(IAbilityIcon icon)
    {
        var instance = Instantiate(iconPrefab, abilityIconSlots[GetActiveAbilityCount()]);
        instance.transform.localPosition = Vector3.zero;
        var newIcon = instance.GetComponent<ActiveAbilityIcon>();
        newIcon.SetIcon(icon);

        return newIcon;
    }
}
