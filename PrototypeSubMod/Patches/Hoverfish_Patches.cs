using HarmonyLib;

[HarmonyPatch(typeof(Hoverfish))]
internal class Hoverfish_Patches
{

    [HarmonyPatch(nameof(Hoverfish.Update))] 
    [HarmonyPostfix]
    private static void Start_Postfix(Hoverfish __instance)
    {
        if (!__instance.gameObject.GetComponent <HoverfishInventoryTracker>())
        {
            __instance.gameObject.AddComponent<HoverfishInventoryTracker>();
        }
    }
}
