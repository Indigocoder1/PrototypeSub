using Nautilus.Json;
using PrototypeSubMod.Utility;
using System.Collections.Generic;
using Newtonsoft.Json;
using PrototypeSubMod.Facilities.Engine;
using PrototypeSubMod.PhaseGates;

namespace PrototypeSubMod.SaveData;

internal class ProtoGlobalSaveData : SaveDataCache
{
    [JsonIgnore]
    public bool EngineFacilityPointsRepaired => repairedEngineFacilityPoints.Count >= EngineFacilityRepairPoint.REPAIR_POINTS_COUNT;

    //Key: Prefab identifier ID | Value: Normalized battery charge
    public readonly Dictionary<string, float> normalizedBatteryCharges = new();

    public readonly Dictionary<string, float> deployableLightLifetimes = new();
    public readonly Dictionary<string, int> phaseGateIndices = new();
    public readonly List<string> unlockedCategoriesLastCheck = new();
    public readonly List<string> repairedEngineFacilityPoints = new();
    public readonly List<PhaseGateLocation> phaseGateLocations = new();
    
    public bool prototypePresent;
    public bool prototypeDestroyed;

    public bool insideEngineFacility;
    public bool moonpoolDoorOpened;
    public bool reactorSequenceComplete;
    public bool storyEndPingSpawned;
    public bool hasDockedVehicle;
}
