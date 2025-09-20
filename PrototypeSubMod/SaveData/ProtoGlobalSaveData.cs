using Nautilus.Json;
using PrototypeSubMod.Utility;
using System.Collections.Generic;
using Newtonsoft.Json;
using PrototypeSubMod.Facilities.Engine;

namespace PrototypeSubMod.SaveData;

internal class ProtoGlobalSaveData : SaveDataCache
{
    [JsonIgnore]
    public bool EngineFacilityPointsRepaired => repairedEngineFacilityPoints.Count >= EngineFacilityRepairPoint.REPAIR_POINTS_COUNT;

    //Key: Prefab identifier ID | Value: Normalized battery charge
    public Dictionary<string, float> normalizedBatteryCharges = new();

    public Dictionary<string, float> deployableLightLifetimes = new();
    public List<string> unlockedCategoriesLastCheck = new();
    public List<string> repairedEngineFacilityPoints = new();
    
    public bool prototypePresent;
    public bool prototypeDestroyed;

    public bool insideEngineFacility;
    public bool moonpoolDoorOpened;
    public bool reactorSequenceComplete;
    public bool storyEndPingSpawned;
    public bool hasDockedVehicle;
}
