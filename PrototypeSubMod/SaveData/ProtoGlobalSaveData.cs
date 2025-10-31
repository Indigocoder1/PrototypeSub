using Nautilus.Json;
using PrototypeSubMod.Utility;
using System.Collections.Generic;
using Newtonsoft.Json;
using PrototypeSubMod.Facilities.Engine;
using PrototypeSubMod.PhaseGates;
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace PrototypeSubMod.SaveData;

internal class ProtoGlobalSaveData : SaveDataCache
{
    [JsonIgnore]
    public bool EngineFacilityPointsRepaired => repairedEngineFacilityPoints.Count >= EngineFacilityRepairPoint.REPAIR_POINTS_COUNT;

    //Key: Prefab identifier ID | Value: Normalized battery charge
    public Dictionary<string, float> normalizedBatteryCharges = new();

    public Dictionary<string, float> deployableLightLifetimes = new();
    public Dictionary<string, int> phaseGateIndices = new();
    public Dictionary<string, PhaseGateLocation> phaseGateLocations = new();
    public List<string> unlockedCategoriesLastCheck = new();
    public List<string> repairedEngineFacilityPoints = new();
    public List<string> activatedTransmissionDevices = new();
    
    public bool prototypePresent;
    public bool prototypeDestroyed;

    public bool insideEngineFacility;
    public bool moonpoolDoorOpened;
    public bool reactorSequenceComplete;
    public bool storyEndPingSpawned;
    public bool hasDockedVehicle;
}
