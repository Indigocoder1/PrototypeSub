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
    }

    private static IEnumerator SpawnPrefab(TechType techType, Vector3 position)
    {
        var task = CraftData.GetPrefabForTechTypeAsync(techType);
        yield return task;

        GameObject.Instantiate(task.GetResult(), position, Quaternion.identity);
    }
}