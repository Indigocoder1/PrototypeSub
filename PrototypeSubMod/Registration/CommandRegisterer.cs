using Nautilus.Commands;
using Nautilus.Handlers;
using UnityEngine;
using PrototypeSubMod.Extensions;

namespace PrototypeSubMod.Registration;

internal static class CommandRegisterer
{
    public static void Register()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        foreach ((string name, Vector3 loc) in Plugin.FACILITY_POSITIONS)
        {
            ConsoleCommandsHandler.AddGotoTeleportPosition(name.ToLowerInvariant(), loc);
        }

        ConsoleCommandsHandler.AddGotoTeleportPosition("transmissionsite", Plugin.TransmissionSitePos);
        sw.Stop();
        Plugin.Logger.LogInfo($"Console commands registered in {sw.ElapsedMilliseconds}ms");
    }

    [ConsoleCommand("activateteleporter")]
    public static string ActivateTeleporter(string teleporterID)
    {
        TeleporterManager.ActivateTeleporter(teleporterID);
        ErrorMessage.AddError($"Teleporter '{teleporterID}' activated");
        return string.Empty;
    }
}
