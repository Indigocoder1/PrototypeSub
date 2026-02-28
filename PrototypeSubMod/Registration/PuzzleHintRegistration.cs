using System.Collections;
using Nautilus.Handlers;
using PrototypeSubMod.Prefabs.FacilityProps;
using PrototypeSubMod.PrototypeStory.CalibrationSite;
using UnityEngine;

namespace PrototypeSubMod.Registration;

public static class PuzzleHintRegistration
{
    public static void Register()
    {
        #region Number Puzzle Ping
        var numberPuzzlePingTechType = CustomPing.CreatePing("NumberPuzzlePing", PingType.Signal);
        StoryGoalHandler.RegisterCustomEvent("OnPlayProtoRadioMessage1", () =>
        {
            var numberPuzzlePingPos = new Vector3(-174f, -64f, 307f);
            UWE.CoroutineHost.StartCoroutine(SpawnPrefab(numberPuzzlePingTechType, numberPuzzlePingPos));
        });
        #endregion
        
        #region Bearing Puzzle Ping
        var bearingPingTechType = CustomPing.CreatePing("BearingPuzzlePing", PingType.Signal);
        StoryGoalHandler.RegisterCustomEvent("OnPlayProtoRadioMessage2", () =>
        {
            var bearingPuzzlePingPos = new Vector3(1226, -306, 534);
            UWE.CoroutineHost.StartCoroutine(SpawnPrefab(bearingPingTechType, bearingPuzzlePingPos));
        });
        #endregion
        
        #region Calibration Site Ping

        var calibrationPingTechType = CustomPing.CreatePing("CalibrationSitePing", PingType.Signal,
            components: typeof(DestroyOnCalibrationCompletion));
        StoryGoalHandler.RegisterCustomEvent("OnPlayProtoRadioMessage3", () =>
        {
            UWE.CoroutineHost.StartCoroutine(SpawnPrefab(calibrationPingTechType, CalibrationRunManager.InitialPoint));
        });
        #endregion

        #region Transmission Site Hint

        var transmissionStartTechType = CustomPing.CreatePing("TransmissionSiteStartPing", PingType.Signal);
        var transmissionSiteTechType = CustomPing.CreatePing("TransmissionSitePing", PingType.Signal);
        StoryGoalHandler.RegisterCompoundGoal("ProtoTransmissionSiteHint", Story.GoalType.Story, 20,
            "OnPlayProtoRadioMessage4");
        StoryGoalHandler.RegisterCustomEvent("ProtoTransmissionSiteHint", () =>
        {
            PDALog.Add("ProtoTransmissionSiteHint");
            PDAEncyclopedia.Add("ProtoTransmissionSiteEncy", true);
            UWE.CoroutineHost.StartCoroutine(SpawnPrefab(transmissionStartTechType, Plugin.TransmissionSiteStartPos));
            UWE.CoroutineHost.StartCoroutine(SpawnPrefab(transmissionSiteTechType, Plugin.TransmissionSitePos));
        });

        #endregion
    }

    private static IEnumerator SpawnPrefab(TechType techType, Vector3 position)
    {
        var task = CraftData.GetPrefabForTechTypeAsync(techType);
        yield return task;

        GameObject.Instantiate(task.GetResult(), position, Quaternion.identity);
    }
}