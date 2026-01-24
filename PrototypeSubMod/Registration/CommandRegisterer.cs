using Nautilus.Handlers;
using UnityEngine;

namespace PrototypeSubMod.Registration;

internal static class CommandRegisterer
{
    public static void Register()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        ConsoleCommandsHandler.AddGotoTeleportPosition("interceptorfacility", new Vector3(547, -709, 955));
        ConsoleCommandsHandler.AddGotoTeleportPosition("defensefacility", new Vector3(689, -483, -1404f));
        ConsoleCommandsHandler.AddGotoTeleportPosition("enginefacility", new Vector3(-558, -463, 1497f));
        ConsoleCommandsHandler.AddGotoTeleportPosition("hullfacility", new Vector3(-1182, -443, -1146));
        ConsoleCommandsHandler.AddGotoTeleportPosition("hulloutpost", new Vector3(-162, -69, -226));
        ConsoleCommandsHandler.AddGotoTeleportPosition("ppt", new Vector3(449, -92, 1169));
        ConsoleCommandsHandler.RegisterConsoleCommand("enableworm)", typeof(Commands.PrototypeCommands), nameof(Commands.PrototypeCommands.EnableWorm), null);

        sw.Stop();
        Plugin.Logger.LogInfo($"Console commands registered in {sw.ElapsedMilliseconds}ms");
    }
}
