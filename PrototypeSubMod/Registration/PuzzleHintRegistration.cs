using System.Collections;
using Nautilus.Handlers;
using PrototypeSubMod.Prefabs.FacilityProps;
using UnityEngine;

namespace PrototypeSubMod.Registration;

public static class PuzzleHintRegistration
{
    public static void Register()
    {
        #region Number Puzzle Ping
        StoryGoalHandler.RegisterCustomEvent("OnPlayProtoRadioMessage1", () =>
        {
            var numberPuzzlePingPos = new Vector3(-238f, -68f, 290f);
            var techType = CustomPing.CreatePing("NumberPuzzlePing", PingType.Signal);
            UWE.CoroutineHost.StartCoroutine(SpawnPrefab(techType, numberPuzzlePingPos));
        });
        #endregion
        
        #region Bearing Puzzle Ping
        StoryGoalHandler.RegisterCustomEvent("OnPlayProtoRadioMessage2", () =>
        {
            var bearingPuzzlePingPos = new Vector3(1226, -306, 534);
            var techType = CustomPing.CreatePing("BearingPuzzlePing", PingType.Signal);
            UWE.CoroutineHost.StartCoroutine(SpawnPrefab(techType, bearingPuzzlePingPos));
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