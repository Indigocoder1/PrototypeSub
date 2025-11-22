using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PrototypeSubMod.LightDistortionField;
using PrototypeSubMod.Registration;
using PrototypeSubMod.Teleporter;
using UnityEngine;

namespace PrototypeSubMod.Factors.Tether;

public class SubTetherLogic : Factor
{
    public override GameInput.Button GetUseButton() => InputRegisterer.TetherSubButton;

    private float confirmationWaitPeriod = 1f;

    private float timeAskedToConfirm;
    
    public override void StartUse()
    {
        base.StartUse();
        if (!Plugin.GlobalSaveData.prototypePresent || Plugin.GlobalSaveData.prototypeDestroyed)
        {
            ErrorMessage.AddError("No sub to teleport to!");
            return;
        }

        if (Time.time > timeAskedToConfirm + confirmationWaitPeriod)
        {
            timeAskedToConfirm = Time.time;
            ErrorMessage.AddError("Press again to confirm teleportation");
            return;
        }
        
        UWE.CoroutineHost.StartCoroutine(TeleportPlayer());
    }

    private IEnumerator TeleportPlayer()
    {
        var subRoot = CloakEffectHandler.EffectHandlers[0].GetComponentInParent<SubRoot>();
        var teleporterManager = subRoot.GetComponentInChildren<ProtoTeleporterManager>();

        var player = Player.main;
        
        player.AddUsedTool(TechType.PrecursorTeleporter);
        player.cinematicModeActive = true;
        player.playerController.inputEnabled = false;
        player.GetPDA().Close();
        player.GetPDA().SetIgnorePDAInput(true);
        player.teleportingLoopSound.Play();
        
        Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);

        var teleportPos = teleporterManager.GetTeleportPosition();
        var rotation = Quaternion.Euler(0f, teleportPos.eulerAngles.y, 0f);
        var task = AddressablesUtility.InstantiateAsync(teleporterManager.GetEndCinematicController().RuntimeKey as string, null, teleportPos.position, rotation, true);
        yield return task;
        if (task.GetResult() == null)
        {
            Plugin.Logger.LogError("SubTetherLogic.TeleportPlayer failed: " + gameObject.name);
            Player.main.CompleteTeleportation();
            yield break;
        }

        Camera.main.GetComponent<TeleportScreenFXController>().StartTeleport();
        yield return new WaitForSeconds(1f);
        
        player.transform.position = teleportPos.position;
        player.transform.rotation = rotation;
        player.WaitForTeleportation();
        Player.main.SetPrecursorOutOfWater(false);
        TeleporterOverride.QueuedTeleportedBackToSub = true;
    }
}