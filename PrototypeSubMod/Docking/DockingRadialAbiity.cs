using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.PrototypeStory;
using PrototypeSubMod.UI.AbilitySelection;
using PrototypeSubMod.Upgrades;
using UnityEngine;

namespace PrototypeSubMod.Docking;

internal class DockingRadialAbility : ProtoUpgrade
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private SelectionMenuManager selectionMenuManager;
    [SerializeField] private ProtoFinsManager finsManager;
    [SerializeField] private ProtoDockingManager dockingManager;

    private void Start()
    {
        finsManager.onFinCountChanged += () =>
        {
            if (finsManager.GetInstalledFinCount() < ProtoFinsManager.FinsForDocking) return;

            selectionMenuManager.RefreshIcons();
        };

        dockingManager.onDockedStatusChanged += selectionMenuManager.RefreshIcons;
    }
    
    public override bool OnActivated()
    {
        if (ProtoStoryLocker.StoryEndingActive) return false;

        if (!dockingManager.GetDockingBay().HasUndockingClearance())
        {
            ErrorMessage.AddError(Language.main.Get("ProtoNoDockingClearance"));
            return false;
        }
        
        dockingManager.Undock();
        return true;
    }

    public override void OnSelectedChanged(bool changed) { }
    public override bool GetShouldShow()
    {
        return finsManager.GetInstalledFinCount() >= ProtoFinsManager.FinsForDocking && dockingManager.GetDockingBay().dockedVehicle;
    }

    public override TechType GetTechType() => TechType.None;
    public override Sprite GetSprite() => sprite;
}