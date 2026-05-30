using UnityEngine;

namespace PrototypeSubMod.UI.AbilitySelection;

public interface IAbilityIcon
{
    public bool OnActivated();
    public void OnSelectedChanged(bool changed);
    public bool GetActive();
    public bool GetCanActivate();
    public bool GetShouldShow();
    public bool GetIsInstalled();
    public void ForceDisabled();
    public Sprite GetSprite();
    public TechType GetTechType();
}
