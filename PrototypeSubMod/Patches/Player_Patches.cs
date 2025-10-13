using HarmonyLib;
using PrototypeSubMod.Facilities.Defense;
using PrototypeSubMod.Facilities.Interceptor;
using PrototypeSubMod.LightDistortionField;
using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using PrototypeSubMod.PrototypeStory;
using PrototypeSubMod.Teleporter;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PrototypeSubMod.MiscMonobehaviors;
using PrototypeSubMod.PrecursorWearables;
using PrototypeSubMod.Utility;
using UnityEngine;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(Player))]
internal class Player_Patches
{
    public static GameObject DummyLDFTarget;

    [HarmonyPatch(nameof(Player.Start)), HarmonyPostfix]
    private static void Start_Postfix()
    {
        Player.main.gameObject.AddComponent<FactorManager>();
        Camera.main.gameObject.AddComponent<LightDistortionApplier>();
        Player.main.gameObject.AddComponent<PrecursorSuitManager>();
        Camera.main.gameObject.AddComponent<ProtoScreenTeleporterFXManager>();
        Camera.main.gameObject.AddComponent<CloakCutoutApplier>();

        DummyLDFTarget = new GameObject("DummyLDFTarget");

        var canvas = Plugin.AssetBundle.LoadAsset<GameObject>("ProtoFadeCanvas");
        GameObject.Instantiate(canvas);
    }

    [HarmonyPatch(nameof(Player.CanEject)), HarmonyPostfix]
    private static void CanEject_Postfix(Player __instance, ref bool __result)
    {
        if (__instance.teleportingLoopSound.playing || (ProtoEmergencyWarp.activeWarp && ProtoEmergencyWarp.activeWarp.IsCharging()))
        {
            __result = false;
        }
    }

    [HarmonyPatch(nameof(Player.GetRespawnPosition)), HarmonyPostfix]
    private static void GetRespawnPosition_Postfix(ref Vector3 __result)
    {
        if (!InterceptorIslandManager.Instance.GetIslandEnabled() || !InterceptorReactorSequenceManager.SequenceInProgress) return;

        __result = InterceptorIslandManager.Instance.GetRespawnPoint();
    }

    [HarmonyPatch(nameof(Player.MovePlayerToRespawnPoint)), HarmonyPrefix]
    private static bool MovePlayerToRespawnPoint_Prefix(Player __instance)
    {
        if (!InterceptorIslandManager.Instance.GetIslandEnabled() || !InterceptorReactorSequenceManager.SequenceInProgress) return true;

        __instance.SetPosition(InterceptorIslandManager.Instance.GetRespawnPoint());
        __instance.SetCurrentSub(null);
        return false;
    }

    [HarmonyPatch(nameof(Player.CheckTeleportationComplete)), HarmonyPostfix]
    private static void CheckForTeleportationComplete_Postfix()
    {
        if (!LargeWorldStreamer.main.IsWorldSettled()) return;

        UWE.CoroutineHost.StartCoroutine(ResetTeleporterColors());
    }

    private static IEnumerator ResetTeleporterColors()
    {
        yield return new WaitForSeconds(3f);

        var teleportManager = Camera.main.GetComponent<ProtoScreenTeleporterFXManager>();
        teleportManager.ResetColors();
    }

    [HarmonyPatch(nameof(Player.CalculateBiome)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CalculateBiome_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var outOfWaterField = typeof(Player).GetField("precursorOutOfWater", BindingFlags.Public | BindingFlags.Instance);
        var match = new CodeMatch(i => i.opcode == OpCodes.Ldfld && (FieldInfo)i.operand == outOfWaterField);

        var matcher = new CodeMatcher(instructions)
            .MatchForward(false, match)
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
            .InsertAndAdvance(Transpilers.EmitDelegate(BiomeIsWalkableBlacklisted));

        return matcher.InstructionEnumeration();
    }

    public static readonly List<string> BLACKLISTED_WALKABLE_BIOMES = new()
    {
        Plugin.DEFENSE_CHAMBER_BIOME_NAME,
        Plugin.ENGINE_FACILITY_BIOME_NAME,
        "protohullfacilitycalm",
        "protohullfacilitytense"
    };
    
    public static bool BiomeIsWalkableBlacklisted(bool previousResult, string currentBiome)
    {
        if (!BLACKLISTED_WALKABLE_BIOMES.Contains(currentBiome)) return previousResult;

        return false;
    }
    
    private const int MIN_OX_REQ = 135;

    [SaveStateReference(float.MinValue)]
    private static float TimeWouldHaveDrowned;
    [SaveStateReference(false)]
    private static bool SavedFromDrowning;
    private static bool OverrideOxygen;
    private static float OverrideOxygenAmount;

    [HarmonyPatch(nameof(Player.GetOxygenPerBreath)), HarmonyPostfix]
    private static void GetOxygenPerBreath_Postfix(Player __instance, ref float __result)
    {
        if (OverrideOxygen)
        {
            __result = OverrideOxygenAmount;
            return;
        }

        var biome = __instance.GetBiomeString();
        bool inTunnel = biome.StartsWith("protodefensetunnel");

        if (!inTunnel) return;

        float oxCapacity = __instance.oxygenMgr.GetOxygenCapacity();

        if (oxCapacity < MIN_OX_REQ) return;

        bool hasRebreather = Inventory.main.equipment.GetCount(TechType.Rebreather) > 0;
        float normalizedOx = oxCapacity / MIN_OX_REQ;
        float dividend = normalizedOx <= 1 ? Mathf.Pow(0.727f, normalizedOx - 1) + 0.07f : Mathf.Pow(0.65f, normalizedOx) + 0.1f;
        if (hasRebreather)
        {
            dividend /= 2;
        }

        __result *= 1 / dividend;

        if (__instance.oxygenMgr.GetOxygenAvailable() > 30)
        {
            SavedFromDrowning = false;
        }
        
        bool wouldHaveDrowned = __instance.oxygenMgr.GetOxygenAvailable() - __result <= 1;
        if (wouldHaveDrowned && Time.time > TimeWouldHaveDrowned + 30f && !SavedFromDrowning)
        {
            TimeWouldHaveDrowned = Time.time;
            SavedFromDrowning = true;
        }
        
        if (wouldHaveDrowned && Time.time < TimeWouldHaveDrowned + 30f)
        {
            __result = 0;
        }
    }

    public static void SetOxygenReqOverride(bool overrideOx, float overrideAmount)
    {
        OverrideOxygen = overrideOx;
        OverrideOxygenAmount = overrideAmount;
    }

    [HarmonyPatch(nameof(Player.ExitPilotingMode)), HarmonyPostfix]
    private static void ExitPilotingMode_Postfix(Player __instance)
    {
        if (ProtoStoryLocker.StoryEndingActive)
        {
            __instance.currentSub.GetComponent<SubControl>().Set(SubControl.Mode.DirectInput);
        }
    }
}
