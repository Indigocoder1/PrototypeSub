using System;
using PrototypeSubMod.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PrototypeSubMod.Prefabs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.SubTerminal;

internal class NewUpgradesScreen : MonoBehaviour
{
    [SerializeField] private List<ProtoUpgradeCategory> upgradeCategories;
    [SerializeField] private BuildTerminalScreenManager screenManager;
    [SerializeField] private ProtoUpgradeCategory hullUpgradeCategory;
    [SerializeField] private VoiceNotificationManager manager;
    [SerializeField] private string storyEndPDAKey;
    [SerializeField] private string hullKeyPDAKey;
    [SerializeField] private VoiceNotification newDataNotification;
    [SerializeField] private GameObject buttonObjects;
    [SerializeField] private GameObject downloadingObjects;
    [SerializeField] private Image progressBar;
    [SerializeField] private float downloadLength;
    
    [Header("Sprites")]
    [SerializeField] private Button downloadButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private SpriteData[] spriteDatas;

    [SerializeField, HideInInspector] public ProtoUpgradeCategory[] categories;
    [SerializeField, HideInInspector] public Sprite[] normalButtonSprites;
    [SerializeField, HideInInspector] public Sprite[] hoveredButtonSprites;
    [SerializeField, HideInInspector] public Sprite[] backgroundSprites;

    private List<string> queuedPdaMessages = new();
    private List<ProtoUpgradeCategory> mostRecentCategories;
    private float currentDownloadProgress;
    private bool downloadActive;
    private bool pingSpawnAttempted;

    private Queue<VoiceNotification> queuedVoicelines = new();
    private SpriteData[] serializedSpriteDatas;

    private void OnValidate()
    {
        categories = new ProtoUpgradeCategory[spriteDatas.Length];
        normalButtonSprites = new Sprite[spriteDatas.Length];
        hoveredButtonSprites = new Sprite[spriteDatas.Length];
        backgroundSprites = new Sprite[spriteDatas.Length];

        for (int i = 0; i < spriteDatas.Length; i++)
        {
            var data = spriteDatas[i];
            categories[i] = data.category;
            normalButtonSprites[i] = data.normalButtonSprite;
            hoveredButtonSprites[i] = data.hoveredButtonSprite;
            backgroundSprites[i] = data.backgroundSprite;
        }
    }

    private void Start()
    {
        serializedSpriteDatas = new SpriteData[categories.Length];
        
        for (int i = 0; i < categories.Length; i++)
        {
            var data = new SpriteData
            {
                category = categories[i],
                normalButtonSprite = normalButtonSprites[i],
                hoveredButtonSprite = hoveredButtonSprites[i],
                backgroundSprite = backgroundSprites[i]
            };
            serializedSpriteDatas[i] = data;
        }
        
        mostRecentCategories = GetUnlocksSinceLastCheck();
        if (mostRecentCategories.Count == 0) return;

        var spriteData = serializedSpriteDatas.FirstOrDefault(i => i.category == mostRecentCategories[0]);
        var spriteState = downloadButton.spriteState;
        spriteState.highlightedSprite = spriteData.hoveredButtonSprite;
        spriteState.pressedSprite = spriteData.hoveredButtonSprite;
        downloadButton.spriteState = spriteState;

        ((Image)downloadButton.targetGraphic).sprite = spriteData.normalButtonSprite;
        backgroundImage.sprite = spriteData.backgroundSprite;
    }

    private void Update()
    {
        if (!downloadActive || mostRecentCategories == null || mostRecentCategories.Count == 0) return;

        if (Time.timeScale == 0) return;

        if (currentDownloadProgress < downloadLength)
        {
            currentDownloadProgress += Time.deltaTime;
            float normalizedProgress = currentDownloadProgress / downloadLength;
            progressBar.fillAmount = normalizedProgress;
        }
        else if (!pingSpawnAttempted)
        {
            pingSpawnAttempted = true;
            downloadActive = false;
            screenManager.EnableRelevantScreens();
            queuedVoicelines.Clear();
            queuedVoicelines.Enqueue(newDataNotification);
            SpawnPingIfNeeded();
            UWE.CoroutineHost.StartCoroutine(PlayQueuedVoicelines());
        }
    }

    public void StartDownload()
    {
        progressBar.fillAmount = 0;
        currentDownloadProgress = 0;
        downloadActive = true;
        pingSpawnAttempted = false;
        buttonObjects.SetActive(false);
        downloadingObjects.SetActive(true);
        UpdateStoredUnlocks();
    }

    public List<ProtoUpgradeCategory> GetUnlocksSinceLastCheck()
    {
        var newUnlocks = new List<ProtoUpgradeCategory>();

        foreach (var category in upgradeCategories)
        {
            if (category.GetUnlockedUpgrades().Count > 0 && !Plugin.GlobalSaveData.unlockedCategoriesLastCheck.Contains(category.localizationKey))
            {
                newUnlocks.Add(category);
            }
        }

        return newUnlocks;
    }

    public bool HasQueuedUnlocks()
    {
        return GetUnlocksSinceLastCheck().Count > 0;
    }

    private void UpdateStoredUnlocks()
    {
        List<string> unlockedCategoryKeys = new();
        foreach (var category in GetUnlocksSinceLastCheck())
        {
            unlockedCategoryKeys.Add(category.localizationKey);
        }
        
        Plugin.GlobalSaveData.unlockedCategoriesLastCheck.AddRange(unlockedCategoryKeys);
    }

    public void ResetDownload()
    {
        downloadActive = false;
        buttonObjects.SetActive(true);
        downloadingObjects.SetActive(false);
    }

    private void SpawnPingIfNeeded()
    {
        CheckForStoryPing();
        CheckForHullKey();
        
        screenManager.EndBuildStage();
    }

    private void CheckForHullKey()
    {
        if (KnownTech.Contains(HullFacilityKey.prefabInfo.TechType)) return;
        
        foreach (var item in upgradeCategories)
        {
            if (item == hullUpgradeCategory) continue;
            
            if (!Plugin.GlobalSaveData.unlockedCategoriesLastCheck.Contains(item.localizationKey)) return;
        }

        PDALog.Add(hullKeyPDAKey);
        KnownTech.Add(HullFacilityKey.prefabInfo.TechType);
        PDAEncyclopedia.Add("HullFacilityTabletEncy", true);

        KnownTech.Add(IonPrism_Craftable.prefabInfo.TechType);
        PDAEncyclopedia.Add("ProtoIonPrismEncy", true);
        queuedPdaMessages.Add(hullKeyPDAKey);
        
        // Remove the new upgrades message
        queuedVoicelines.Dequeue();
    }
    
    private void CheckForStoryPing()
    {
        if (Plugin.GlobalSaveData.storyEndPingSpawned)
        {
            return;
        }

        foreach (var item in upgradeCategories)
        {
            if (!Plugin.GlobalSaveData.unlockedCategoriesLastCheck.Contains(item.localizationKey)) return;
        }

        Plugin.GlobalSaveData.storyEndPingSpawned = true;
        UWE.CoroutineHost.StartCoroutine(SpawnStoryEndPing());
        PDALog.Add(storyEndPDAKey);
        queuedPdaMessages.Add(storyEndPDAKey);
        IngameMenu.main.SaveGame();
    }
    
    private IEnumerator SpawnStoryEndPing()
    {
        var task = CraftData.GetPrefabForTechTypeAsync(Plugin.StoryEndPingTechType);
        yield return task;

        var prefab = task.GetResult();
        Instantiate(prefab, Plugin.StoryEndPos, Quaternion.identity);
    }

    private IEnumerator PlayQueuedVoicelines()
    {
        float delay = 0;
        foreach (var message in queuedPdaMessages)
        {
            // Make Nautilus refresh metadata
            Language.main.Contains(message);
            
            var data = Language.main.GetMetaData(message);
            for (int i = 0; i < data.lineCount; i++)
            {
                delay += data.GetLine(i).duration;
            }
        }

        yield return new WaitForSeconds(delay + 1f);
        
        while (queuedVoicelines.Count > 0)
        {
            var line = queuedVoicelines.Dequeue();
            manager.PlayVoiceNotification(line, false, true);

            yield return new WaitForSeconds(line.minInterval);
        }
    }

    [System.Serializable]
    private class SpriteData
    {
        public ProtoUpgradeCategory category;
        public Sprite normalButtonSprite;
        public Sprite hoveredButtonSprite;
        public Sprite backgroundSprite;
    }

    private void OnEnable() => ResetDownload();
}
