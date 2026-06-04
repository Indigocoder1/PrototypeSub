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
        #region Number Puzzle Hint
        StoryGoalHandler.RegisterCompoundGoal("ProtoNumberPuzzleHint", Story.GoalType.Encyclopedia, 20,
            "OnPlayProtoRadioMessage1");
        
        #endregion
        
        #region Bearing Puzzle Hint
        StoryGoalHandler.RegisterCompoundGoal("ProtoBearingPuzzleHint", Story.GoalType.Encyclopedia, 20,
            "OnPlayProtoRadioMessage2");
        #endregion

        #region Calibration Site Ping

        var calibrationPingTechType = CustomPing.CreatePing("CalibrationSitePing", Plugin.HintPingType,
            visitable: false, components: typeof(DestroyOnCalibrationCompletion));
        StoryGoalHandler.RegisterCustomEvent("OnPlayProtoRadioMessage3", () =>
        {
            UWE.CoroutineHost.StartCoroutine(SpawnPrefab(calibrationPingTechType, CalibrationRunManager.InitialPoint));
        });
        #endregion

        #region Transmission Site Hint

        var transmissionStartTechType = CustomPing.CreatePing("TransmissionSiteStartPing", Plugin.HintPingType, visitable: false);
        var transmissionSiteTechType = CustomPing.CreatePing("TransmissionSitePing", Plugin.HintPingType);
        StoryGoalHandler.RegisterCompoundGoal("ProtoTransmissionSiteHint", Story.GoalType.Story, 20,
            "OnPlayProtoRadioMessage4");
        StoryGoalHandler.RegisterCustomEvent("ProtoTransmissionSiteHint", () =>
        {
            PDALog.Add("ProtoTransmissionSiteHint");
            PDAEncyclopedia.Add("ProtoTransmissionSiteEncy", true);
            PDAEncyclopedia.Add("TransmissionSiteHint", true);
        });

        #endregion
    }

    public static IEnumerator SpawnPrefab(TechType techType, Vector3 position)
    {
        var task = CraftData.GetPrefabForTechTypeAsync(techType);
        yield return task;

        GameObject.Instantiate(task.GetResult(), position, Quaternion.identity);
    }
}