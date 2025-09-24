using System;
using System.Collections;
using Newtonsoft.Json;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.StructureLoading;
using UnityEngine;

namespace PrototypeSubMod.Registration;

internal static class StructureRegisterer
{
    public static IEnumerator Register()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        
        yield return Structure.RegisterFromBundle("DefenseTunnel");
        yield return Structure.RegisterFromBundle("EngineFacilityObjects");
        yield return Structure.RegisterFromBundle("EngineFacilityAdditions");
        yield return Structure.RegisterFromBundle("EngineFacilityExteriorObjects");
        yield return Structure.RegisterFromBundle("DefenseMoonpool");
        yield return Structure.RegisterFromBundle("ProtoItemDisplayCases");
        yield return Structure.RegisterFromBundle("ProtoIslands");
        yield return Structure.RegisterFromBundle("ProtoWarpCore");
        yield return Structure.RegisterFromBundle("DefenseFacilityDebris");
        yield return Structure.RegisterFromBundle("HullFacilityOutpost");
        yield return Structure.RegisterFromBundle("PrecursorFabricators");
        yield return Structure.RegisterFromBundle("HullFacilityObjects");
        yield return Structure.RegisterFromBundle("HullFacilityTunnelExtras");
        yield return Structure.RegisterFromBundle("HullFacilityKeyResources");
        yield return Structure.RegisterFromBundle("ProtoHullCave");
        yield return Structure.RegisterFromBundle("DefenseFacilityTeleporterRoomExteriorObjects");
        yield return Structure.RegisterFromBundle("DefenseFacilityTeleporterRoomInteriorObjects");
        yield return Structure.RegisterFromBundle("QEPRemnantSpawns");
        yield return Structure.RegisterFromBundle("EngineFacilityRemnant");

        if (TRPCompatManager.TRPInstalled)
        {
            yield return Structure.RegisterFromBundle("RedPlagueProtoIslands");
        }

        sw.Stop();
        Plugin.Logger.LogInfo($"Structures loaded in {sw.ElapsedMilliseconds}ms");
    }
}
