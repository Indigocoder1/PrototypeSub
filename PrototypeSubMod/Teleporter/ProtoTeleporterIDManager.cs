using System.Collections;
using System.Collections.Generic;
using PrototypeSubMod.SaveData;
using Story;
using SubLibrary.SaveData;
using TMPro;
using UnityEngine;

namespace PrototypeSubMod.Teleporter;

public class ProtoTeleporterIDManager : MonoBehaviour, ISaveDataListener
{
    private static readonly int ScreenActive = Animator.StringToHash("ScreenActive");
    
    [SerializeField] private ProtoTeleporterManager positionSetter;
    [SerializeField] private TeleporterModeManager modeManager;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform surfaceTeleportersParent;
    [SerializeField] private Transform depthsTeleportersParent;
    [SerializeField] private List<string> pcfTeleporterIds;
    [SerializeField] private List<string> blacklistedIds;

    private readonly Dictionary<string, TeleporterLocationItem> locationItems = new();
    private bool screenActive;

    private string activatedPCFTeleporterID;
    
    private void Start()
    {
        PopulateLocationList();
        RefreshLocationList();
    }

    private void PopulateLocationList()
    {
        foreach (var locationItem in surfaceTeleportersParent.GetComponentsInChildren<TeleporterLocationItem>())
        {
            locationItems.Add(locationItem.GetTeleporterID(), locationItem);
            locationItem.gameObject.SetActive(false);
        }
        
        foreach (var locationItem in depthsTeleportersParent.GetComponentsInChildren<TeleporterLocationItem>())
        {
            locationItems.Add(locationItem.GetTeleporterID(), locationItem);
            locationItem.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        TeleporterManager.TeleporterActivateEvent += OnTeleporterActivated;
        RefreshLocationList();
    }

    private void OnDisable()
    {
        TeleporterManager.TeleporterActivateEvent -= OnTeleporterActivated;
    }

    private void OnTeleporterActivated(string identifier)
    {
        RefreshLocationList();
    }

    private void RefreshLocationList()
    {
        foreach (var item in TeleporterManager.main.activeTeleporters)
        {
            string lowercaseID = item.ToLower();
            if (item == "prototypetp") continue;

            if (string.IsNullOrEmpty(activatedPCFTeleporterID) && pcfTeleporterIds.Contains(lowercaseID))
            {
                activatedPCFTeleporterID = lowercaseID;
            }
            
            string keySource = item + "M";
            string keyTarget = item + "S";
            bool blacklistedPCF = !string.IsNullOrEmpty(activatedPCFTeleporterID) && activatedPCFTeleporterID != item && pcfTeleporterIds.Contains(lowercaseID);
            bool blacklistedSource = blacklistedIds.Contains(keySource);
            bool blacklistedTarget = blacklistedIds.Contains(keyTarget);
            
            if (locationItems.TryGetValue(keySource, out TeleporterLocationItem locationItemM) && !blacklistedPCF && !blacklistedSource)
            {
                locationItemM.gameObject.SetActive(true);
            }
            
            if (locationItems.TryGetValue(keyTarget, out TeleporterLocationItem locationItemS) && !blacklistedTarget)
            {
                locationItemS.gameObject.SetActive(true);
            }
        }
    }

    public void OnItemSelected(string id, bool isHost)
    {
        if (id == null)
        {
            modeManager.SetInterfloorMode();
            return;
        }

        modeManager.SetNormalMode();
        
        positionSetter.SetTeleporterID(id);
        foreach (var locationItem in locationItems)
        {
            if (locationItem.Key == id) continue;

            locationItem.Value.SetSelected(false);
        }

        SetScreenActive(false);
    }

    public void UnselectAll()
    {
        foreach (var locationItem in locationItems)
        {
            locationItem.Value.SetSelected(false);
        }
    }

    public void ToggleScreenActive()
    {
        SetScreenActive(!screenActive);
    }

    private void SetScreenActive(bool active)
    {
        screenActive = active;
        animator.SetBool(ScreenActive, screenActive);

        if (active)
        {
            StoryGoalManager.main.OnGoalComplete("ProtoOpenInterceptorMap");
        }
    }

    public void OnSaveDataLoaded(BaseSubDataClass saveData)
    {
        activatedPCFTeleporterID = saveData.EnsureAsPrototypeData().activatedPCFTeleporterID;
    }

    public void OnBeforeDataSaved(ref BaseSubDataClass saveData)
    {
        saveData.EnsureAsPrototypeData().activatedPCFTeleporterID = activatedPCFTeleporterID;
    }
}
