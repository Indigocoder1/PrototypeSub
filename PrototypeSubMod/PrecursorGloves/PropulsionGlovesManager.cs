using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PrototypeSubMod.Prefabs;
using UnityEngine;

namespace PrototypeSubMod.PrecursorGloves;

public class PropulsionGlovesManager : MonoBehaviour
{
    private static readonly int HoldingFlare = Animator.StringToHash("holding_flare");
    
    private bool toolActive;
    private bool wasGrabbingObject;

    private GameObject ikTargetHolder;
    private GameObject ikTarget;
    private PropulsionCannon propulsionCannon;

    private IEnumerator Start()
    {
        UpdateToolActive();
        var propulsionCannonTask = CraftData.GetPrefabForTechTypeAsync(TechType.PropulsionCannon);
        yield return propulsionCannonTask;

        Type[] whitelistedComponents =
        {
            typeof(Transform),
            typeof(PropulsionCannon),
            typeof(EnergyInterface),
            typeof(VFXController),
            typeof(FMOD_CustomLoopingEmitter),
            typeof(Animator),
            typeof(LineRenderer),
            typeof(VFXElectricLine)
        };

        var originalCannon = propulsionCannonTask.result.Get().GetComponent<PropulsionCannon>();
        var propCannon = Instantiate(originalCannon.gameObject, transform);
        DisplayCaseProp.TrimComponents(propCannon, whitelistedComponents.ToList());
        
        propCannon.AddComponent<NoPropulsionEnergyTag>();
        var newGrabEffect = Instantiate(originalCannon.grabbedEffect, propCannon.transform, false);

        propulsionCannon = propCannon.GetComponent<PropulsionCannon>();
        propulsionCannon.grabbedEffect = newGrabEffect;

        var precursorGreen = new Color(0.3277f, 0.9277f, 0.4286f);
        newGrabEffect.GetComponent<Renderer>().material.color = precursorGreen;

        yield return new WaitUntil(() => propulsionCannon.elecLines != null);
        foreach (var electricLine in propulsionCannon.elecLines)
        {
            electricLine.originForce = 0.5f;
            electricLine.GetComponent<Renderer>().material.color = precursorGreen;
            electricLine.GetComponent<LineRenderer>().startWidth = 0.05f;
        }

        ikTargetHolder = new GameObject("PropulsionIKTargetHolder");
        ikTargetHolder.transform.SetParent(Player.main.armsController.transform, false);
        ikTargetHolder.transform.localPosition = new Vector3(0.17f, -0.1f, -0.06f);
        ikTargetHolder.transform.localRotation = Quaternion.identity;

        ikTarget = new GameObject("PropulsionIKTarget");
        ikTarget.transform.SetParent(ikTargetHolder.transform, false);
        ikTarget.transform.localPosition = new Vector3(0f, 0, 0.43f);
        ikTarget.transform.localEulerAngles = new Vector3(4.28f, 80f, 145f);
    }

    public void UpdateToolActive()
    {
        bool holdingItem = Inventory.main.quickSlots.heldItem != null;
        var glovesSlotItem = Inventory.main.equipment.GetItemInSlot("Gloves");
        bool wearingGloves = glovesSlotItem != null &&
                             glovesSlotItem.techType == PrecursorPropulsionGloves.PrefabInfo.TechType;
        if (!holdingItem && wearingGloves && !Player.main.armsController.IsBleederAttached())
        {
            toolActive = true;
        }
        else
        {
            toolActive = false;
            
            if (!propulsionCannon) return;
            propulsionCannon.ReleaseGrabbedObject();
            UpdateAnimationState(false);
        }
    }

    private void Update()
    {
        if (Player.main.armsController.IsBleederAttached() && toolActive)
        {
            UpdateToolActive();
            return;
        }
            
        if (!Player.main.IsFreeToInteract() || Player.main.IsInSub()) return;
        if (!toolActive || !propulsionCannon) return;
        
        HandleTooltips();
        propulsionCannon.UpdateActive();
        
        if (GameInput.GetButtonDown(GameInput.Button.RightHand))
        {
            propulsionCannon.OnShoot();
        }

        bool isGrabbingObject = propulsionCannon.IsGrabbingObject();
        if (GameInput.GetButtonDown(GameInput.Button.AltTool) && isGrabbingObject)
        {
            propulsionCannon.ReleaseGrabbedObject();
        }

        if (isGrabbingObject != wasGrabbingObject)
        {
            UpdateAnimationState(isGrabbingObject);
        }

        if (propulsionCannon.grabbedObject)
        {
            ikTargetHolder.transform.LookAt(propulsionCannon.grabbedObject.transform);
        }

        propulsionCannon.muzzle.position = Player.main.armsController.rightHand.position;
        wasGrabbingObject = isGrabbingObject;
    }

    private void HandleTooltips()
    {
        var text1 = string.Empty;
        var text2 = string.Empty;
        if (propulsionCannon.IsGrabbingObject())
        {
            text1 = LanguageCache.GetButtonFormat("PropulsionCannonToShoot", GameInput.Button.RightHand);
            text2 = Language.main.GetFormat("PropulsionGlovesRelease", GameInput.FormatButton(GameInput.Button.AltTool));
        }
        else
        {
            text1 = LanguageCache.GetButtonFormat("PropulsionCannonToGrab", GameInput.Button.RightHand);
        }

        var useText = text2 == string.Empty ? text1 : $"{text1}, {text2}"; 
        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, useText);
    }

    private void UpdateAnimationState(bool isGrabbingObject)
    {
        if (!isGrabbingObject)
        {
            Player.main.armsController.SetWorldIKTarget(null, null);
            Player.main.playerAnimator.SetBool(HoldingFlare, false);
            return;
        }
        
        Player.main.armsController.SetWorldIKTarget(null, ikTarget.transform);
        Player.main.playerAnimator.SetBool(HoldingFlare, true);
    }
}