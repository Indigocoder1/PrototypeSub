using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PrototypeSubMod.LightDistortionField;
using PrototypeSubMod.Registration;
using PrototypeSubMod.Teleporter;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        
        var subRoot = CloakEffectHandler.EffectHandlers[0].GetComponentInParent<SubRoot>();
        var teleporterManager = subRoot.GetComponentInChildren<ProtoTeleporterManager>();
        var teleportPos = teleporterManager.GetTeleportPosition();
        
        UWE.CoroutineHost.StartCoroutine(TeleportToLocation(teleportPos.position, teleportPos.eulerAngles.y, teleporterManager.GetEndCinematicController()));
        TeleporterOverride.QueuedTeleportedBackToSub = true;
    }

    public IEnumerator TeleportToLocation(Vector3 position, float yAngle, AssetReferenceGameObject endCinematic = null)
    {
        var player = Player.main;
        
        player.AddUsedTool(TechType.PrecursorTeleporter);
        player.cinematicModeActive = true;
        player.playerController.inputEnabled = false;
        player.GetPDA().Close();
        player.GetPDA().SetIgnorePDAInput(true);
        player.teleportingLoopSound.Play();
        
        Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);
        
        var rotation = Quaternion.Euler(0f, yAngle, 0f);
        if (endCinematic != null)
        {
            var task = AddressablesUtility.InstantiateAsync(endCinematic.RuntimeKey as string, null, position, rotation);
            yield return task;
            if (task.GetResult() == null)
            {
                Plugin.Logger.LogError("SubTetherLogic.TeleportPlayer failed: " + gameObject.name);
                Player.main.CompleteTeleportation();
                yield break;
            }
        }

        Camera.main.GetComponent<TeleportScreenFXController>().StartTeleport();
        yield return new WaitForSeconds(1f);
        
        player.transform.position = position;
        player.transform.rotation = rotation;
        player.WaitForTeleportation();
        Player.main.SetPrecursorOutOfWater(false);
    }
}