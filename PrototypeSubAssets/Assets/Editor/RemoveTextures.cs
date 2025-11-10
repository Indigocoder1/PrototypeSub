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
public class RemoveTextures : PipelineJob
{
    private readonly string TEMP_MAT_DIRECTORY = "Assets\\TempMaterials";
    
    private readonly string[] materialWhitelist = new[]
    {
        "precursor_interior_tiles_00",
        "precursor_interior_tiles_00_dark",
        "precursor_interior_tiles_04_thermal_reactor_dark"
    };
    
    private readonly List<GameObject> prefabs = new List<GameObject>();
    private readonly List<Material> matList = new List<Material>();
    
    public override Task Execute(Pipeline pipeline)
    {
        prefabs.Clear();
        matList.Clear();

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

        //Copy materials to temp directory, and make them blank templates
        Directory.CreateDirectory(TEMP_MAT_DIRECTORY);
        
        foreach (var material in matList)
        {
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(material), TEMP_MAT_DIRECTORY + "\\" +  material.name + ".mat");
        }
        
        //Apply the blank materials to our prefabs in the appropriate places
        foreach (var prefab in prefabs)
        {
            foreach (var renderer in prefab.GetAllComponentsInChildren<Renderer>())
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == null)
                        continue;

                    foreach (var material in matList)
                    {
                        if (renderer.sharedMaterials[i].name.Equals(material.name))
                        {
                            renderer.sharedMaterials[i].color = Color.white;

                            var splitSections = renderer.sharedMaterials[i].name.Split('_');
                            var whitelistedTex = string.Empty;
                            if (splitSections[splitSections.Length - 1].StartsWith("KEEP"))
                            {
                                whitelistedTex = splitSections[splitSections.Length - 1].Replace("KEEP", string.Empty);
                                whitelistedTex = whitelistedTex.Split(' ')[0];
                            }

                            var textureNames = renderer.sharedMaterials[i].GetTexturePropertyNames();
                            int index = 0;
                            foreach (var textureID in renderer.sharedMaterials[i].GetTexturePropertyNameIDs())
                            {
                                if (textureNames[index].EndsWith(whitelistedTex))
                                {
                                    index++;
                                    continue;
                                }
                                
                                renderer.sharedMaterials[i].SetTexture(textureID, null);
                                index++;
                            }

                            break;
                        }
                    }
                }
            }
        }
        
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

                foreach (var mat in prefabMaterials)
                {
                    if(!matList.Contains(mat))
                        matList.Add(mat);
                }
                
                /*
                 * Could check for duplicate prefabs here, but since we don't currently have any of those, and there's
                 * a chance we might end up giving different prefabs the same name by mistake in the future, I think
                 * it's better not to, at least for now.
                 */
                prefabs.Add(gameObject);
            }
        }
    
        foreach (var subDirectory in Directory.GetDirectories(directory))
        {
            GetAllPrefabsInDirectory(subDirectory);
        }
    }
    
}
