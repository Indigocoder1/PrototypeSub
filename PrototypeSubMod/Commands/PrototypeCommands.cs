using System.Collections;
using System.Collections.Generic;
using Nautilus.Commands;
using Nautilus.Handlers;
using PrototypeSubMod.Facilities.Hull;
using PrototypeSubMod.Upgrades;
using UnityEngine;

namespace PrototypeSubMod.Commands;

internal static class PrototypeCommands
{
    private static readonly Vector3 SafePos = new Vector3(-633, -65, -79);
    private const int SafeXOffset = 20;

    [ConsoleCommand("unstuckprototype")]
    public static string UnstuckPrototype()
    {
        var upgradeManagers = GameObject.FindObjectsOfType<ProtoUpgradeManager>();
        for (int i = 0; i < upgradeManagers.Length; i++)
        {
            var sub = upgradeManagers[i].gameObject;
            sub.transform.position = SafePos + new Vector3(SafeXOffset * i, 0, 0);
            sub.transform.rotation = Quaternion.identity;
        }

        return string.Empty;
    }

    [ConsoleCommand("resetwormtimer")]
    public static string ResetWormTimer()
    {
        ErrorMessage.AddError("Proto worm timer reset!");
        WormSpawnEvent.ResetSpawnTimer();
        return string.Empty;
    }

    [ConsoleCommand("enableworm")]
        public static string EnableWorm()
    {
        Story.StoryGoalManager.main.OnGoalComplete("HullFacilityWormTerminalEncy");
        Plugin.Logger.LogInfo("Hull Facility Wyrm Enabled");
        ErrorMessage.AddError("Hull Facility Wyrm Enabled");
        return string.Empty;
    }
    

    [ConsoleCommand("protoscreenshot")]
    public static string Screenshot(string path, int superSize)
    {
        ScreenCapture.CaptureScreenshot(path, superSize);
        return string.Empty;
    }

    [ConsoleCommand("radialfps")]
    public static string RadialFPS()
    {
        var fpsCounter = GameObject.FindObjectOfType<FPSCounter>();
        fpsCounter.enabled = true;
        UWE.CoroutineHost.StartCoroutine(LogFPS(fpsCounter));
        return string.Empty;
    }

    private static IEnumerator LogFPS(FPSCounter fpsCounter)
    {
        Plugin.Logger.LogInfo("------------------------------------------");
        var player = Player.main;
        player.cinematicModeActive = true;
        player.FreezeStats();
        player.playerController.SetEnabled(false);
        var mainCameraTrans = Camera.main.transform;
        const int rotationIncrements = 8;
        mainCameraTrans.localEulerAngles = Vector3.zero;
        for (int j = 0; j < rotationIncrements; j++)
        {
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }
        
            Plugin.Logger.LogInfo($"{1 / fpsCounter.avgFrameTime} fps at {mainCameraTrans.eulerAngles.y} degrees");
            mainCameraTrans.eulerAngles += new Vector3(0, 360f / rotationIncrements, 0);
        }

        player.cinematicModeActive = false;
        player.UnfreezeStats();
        player.playerController.SetEnabled(true);
        fpsCounter.enabled = false;
    }
    
    [ConsoleCommand("noghost")]
    public static string NoGhost()
    {
        var ghostSpawners = GameObject.FindObjectsOfType<VoidGhostLeviathansSpawner>();
        var ghosts = GameObject.FindObjectsOfType<GhostLeviatanVoid>();

        for (int i = ghosts.Length - 1; i >= 0; i--)
        {
            GameObject.Destroy(ghosts[i].gameObject);
        }
        
        for (int i = ghostSpawners.Length - 1; i >= 0; i--)
        {
            GameObject.Destroy(ghostSpawners[i].gameObject);
        }
        
        return string.Empty;
    }
}
