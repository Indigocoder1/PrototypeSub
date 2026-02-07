using System;
using System.Collections;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using PrototypeSubMod.Compatibility;
using System.IO;
using System.Threading;
using PrototypeSubMod.MiscMonobehaviors.Materials;
using UnityEngine;

namespace PrototypeSubMod.Utility;

internal static class LoadEasyPrefabs
{
    public static event Action<float> OnProgressChanged;
    
    private static float _progress;
    private static float _bundleProgress;
    
    public static IEnumerator LoadPrefabs(AssetBundle assetBundle, params Action[] onCompleted)
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        
        var assetsRequest = assetBundle.LoadAllAssetsAsync(typeof(EasyPrefab));
        while (!assetsRequest.isDone)
        {
            _bundleProgress = assetsRequest.progress;
            OnProgressChanged?.Invoke(GetLoadProgress());
            yield return assetsRequest;
        }

        _bundleProgress = 1;

        int completedPrefabs = 0;
        foreach (var easyPrefab in assetsRequest.allAssets)
        {
            yield return RegisterEasyPrefab((EasyPrefab)easyPrefab, onCompleted);
            completedPrefabs++;
            _progress = (float)completedPrefabs / assetsRequest.allAssets.Length;
            OnProgressChanged?.Invoke((_bundleProgress + _progress) / 2);
        }
        
        foreach (var action in onCompleted)
        {
            action?.Invoke();
        }
        
        sw.Stop();
        Plugin.Logger.LogInfo($"Easy prefabs fully started in {sw.ElapsedMilliseconds}ms");
    }

    public static void ClearProgressEvents()
    {
        OnProgressChanged = null;
    }

    public static IEnumerator RegisterEasyPrefab(EasyPrefab easyPrefab, Action[] onCompleted)
    {
        PrefabInfo info = PrefabInfo.WithTechType(easyPrefab.techType.techTypeName, null, null, unlockAtStart: easyPrefab.unlockAtStart);
        if (easyPrefab.sprite != null)
        {
            info = info.WithIcon(easyPrefab.sprite);
        }

        var prefab = new CustomPrefab(info); 
        if (easyPrefab.prefab)
        {
            yield return RegisterPrefabWithObject(easyPrefab, prefab, onCompleted);
        }
        else
        {
            SetupMiscellaneousValues(easyPrefab, prefab);

            prefab.Register();
        }
    }

    public static float GetLoadProgress()
    {
        return (_bundleProgress + _progress) / 2;
    }

    private static void SetupMiscellaneousValues(EasyPrefab easyPrefab, CustomPrefab prefab)
    {
        if (easyPrefab.craftable)
        {
            var recipePath = Path.Combine(easyPrefab.jsonRecipePath);
            string path = Path.Combine(recipePath, $"{easyPrefab.techType.techTypeName}.json");
            prefab.SetRecipe(ROTACompatManager.GetRelevantRecipe(path));
        }

        if (easyPrefab.unlockAtStart)
        {
            prefab.SetUnlock(TechType.None);
        }

        if (easyPrefab.isProtoUpgrade)
        {
            prefab.SetPdaGroupCategory(Plugin.PrototypeGroup, Plugin.ProtoModuleCategory);
        }
        else if (!string.IsNullOrEmpty(easyPrefab.techGroup) && !string.IsNullOrEmpty(easyPrefab.techCategory))
        {
            var techGroup = (TechGroup)Enum.Parse(typeof(TechGroup), easyPrefab.techGroup);
            var techCategory = (TechCategory)Enum.Parse(typeof(TechCategory), easyPrefab.techCategory);
            prefab.SetPdaGroupCategory(techGroup, techCategory);
        }
    }

    private static IEnumerator RegisterPrefabWithObject(EasyPrefab easyPrefab, CustomPrefab prefab, Action[] onCompleted)
    {
        if (easyPrefab.applySNShaders)
        {
            if (easyPrefab.applyPrecursorMaterialChanges)
            {
                MaterialUtils.ApplySNShaders(easyPrefab.prefab, modifiers: new ProtoMaterialModifier(1));
            }
            else
            {
                MaterialUtils.ApplySNShaders(easyPrefab.prefab, modifiers: new DefaultMaterialModifier());
            }
        }

        if (easyPrefab.prefab.GetComponentsInChildren<Renderer>(true).Length > 0)
        {
            yield return ProtoMatDatabase.ReplaceVanillaMats(easyPrefab.prefab);
        }
        
        SetupMiscellaneousValues(easyPrefab, prefab);
        
        prefab.SetGameObject(easyPrefab.prefab);
        
        prefab.Register();
    }
}
