using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[PipelineSupport(typeof(Pipeline))]
public class ReplaceTextures : PipelineJob
{
    private readonly string TEMP_MAT_DIRECTORY = Path.Combine("Assets", "TempMaterials");
    
    private readonly string[] materialWhitelist = new[]
    {
        "precursor_interior_tiles_00",
        "precursor_interior_tiles_00_dark",
        "precursor_interior_tiles_04_thermal_reactor_dark"
    };
    
    private readonly List<GameObject> prefabs = new List<GameObject>();
    
    public override Task Execute(Pipeline pipeline)
    {
        if (!Directory.Exists(TEMP_MAT_DIRECTORY))
        {
            Debug.LogError("REPLACE TEXTURES JOB FAILED! The TemporaryMaterials directory is missing!");
            return Task.CompletedTask;
        }
        
        prefabs.Clear();

        var manifest = pipeline.manifest;
        var asmDefDatum = (AssetBundleDefinitions) manifest.Data[1];

        var bundleDirectories = new List<string>();

        foreach (var asset in asmDefDatum.assetBundles[0].assets)
        {
            var directoryPath = Path.Combine("Assets", asset.name);

            if (!Directory.Exists(directoryPath))
                continue;
            
            bundleDirectories.Add(directoryPath);
        }

        foreach (var directory in bundleDirectories)
        {
            GetAllPrefabsInDirectory(directory);
        }
        
        var preservedMaterials = new List<Material>();
        foreach (var file in Directory.GetFiles(TEMP_MAT_DIRECTORY))
        {
            var matAsset = AssetDatabase.LoadAssetAtPath(file, typeof(Material));

            if (matAsset == null)
                continue;

            if (matAsset is Material material)
                preservedMaterials.Add(material);
        }

        foreach (var prefab in prefabs)
        {
            foreach (var renderer in prefab.GetAllComponentsInChildren<Renderer>())
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == null)
                        continue;

                    foreach (var material in preservedMaterials)
                    {
                        if (renderer.sharedMaterials[i].name.Equals(material.name))
                        {
                            renderer.sharedMaterials[i].color = material.color;

                            foreach (var textureID in renderer.sharedMaterials[i].GetTexturePropertyNameIDs())
                                renderer.sharedMaterials[i].SetTexture(textureID, material.GetTexture(textureID));

                            EditorUtility.SetDirty(renderer.sharedMaterials[i]);
                            break;
                        }
                    }
                }
            }
        }

        var metaFile = new FileInfo(TEMP_MAT_DIRECTORY + ".meta");
        
        Directory.Delete(TEMP_MAT_DIRECTORY, true);
        
        if(metaFile.Exists)
            metaFile.Delete();

        AssetDatabase.SaveAssets();

        return Task.CompletedTask;
    }
    
    private void GetAllPrefabsInDirectory(string directory)
    {
        var files = Directory.GetFiles(directory);
    
        foreach(var file in files)
        {
            var asset = AssetDatabase.LoadAssetAtPath(file, typeof(Object));
    
            if (asset == null)
                continue;
    
            if (asset is GameObject gameObject)
            {
                if (file.EndsWith(".fbx") || gameObject.GetAllComponentsInChildren<Renderer>().Length == 0)
                    continue;
                
                var prefabMaterials = new List<Material>();
    
                foreach (var renderer in gameObject.GetAllComponentsInChildren<Renderer>())
                {
                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (material == null)
                            continue;
                        
                        var assetPath = AssetDatabase.GetAssetPath(material);

                        if (assetPath.LastIndexOf('/') <= 0)
                            continue;
                        
                        if (!prefabMaterials.Contains(material) && Array.IndexOf(materialWhitelist, material.name) != -1)
                            prefabMaterials.Add(material);
                    }
                }
    
                if (prefabMaterials.Count == 0)
                    continue;
                
                prefabs.Add(gameObject);
            }
        }
    
        foreach (var subDirectory in Directory.GetDirectories(directory))
        {
            GetAllPrefabsInDirectory(subDirectory);
        }
    }
    
}
