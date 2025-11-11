using HarmonyLib;
using PrototypeSubMod.Utility;

namespace PrototypeSubMod.Patches;

[HarmonyPatch(typeof(PlayerController))]
public class PlayerController_Patches
{
    [SaveStateReference(false)]
    private static bool BlockMotorModeAssignment;
    
    [HarmonyPatch(nameof(PlayerController.SetMotorMode)), HarmonyPrefix]
    private static bool SetMotorMode_Prefix()
    {
        return !BlockMotorModeAssignment;
    }

    public static void SetBlockMotorModeAssignment(bool blockMotorModeAssignment)
    {
        BlockMotorModeAssignment = blockMotorModeAssignment;
    }
}