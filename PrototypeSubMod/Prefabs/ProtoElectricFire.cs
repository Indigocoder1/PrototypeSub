using System.ComponentModel;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public static class ProtoElectricFire
{
    public static PrefabInfo PrefabInfo;

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("ProtoElectricFire", null, null);

        var prefab = new CustomPrefab(PrefabInfo);
        var cloneTemplate = new CloneTemplate(PrefabInfo, "ff8e782e-e6f3-40a6-9837-d5b6dcce92bc");
        cloneTemplate.ModifyPrefab += instance =>
        {
            var mixin = instance.EnsureComponent<LiveMixin>();
            mixin.data = Plugin.GeneralAssetBundle.LoadAsset<LiveMixinData>("PrototypeFireMixin");
            mixin.health = mixin.maxHealth;

            var extinguishableFire = instance.EnsureComponent<VFXExtinguishableFire>();
            extinguishableFire.elements = new VFXExtinguishableFire.FireElement[3];
            var element0 = new VFXExtinguishableFire.FireElement
            {
                gameObject = instance.gameObject,
                enable = true,
                hasParticles = true
            };
            extinguishableFire.elements[0] = element0;
            for (int i = 0; i < 2; i++)
            {
                var element = new VFXExtinguishableFire.FireElement
                {
                    gameObject = instance.transform.GetChild(i).gameObject,
                    enable = true,
                    hasParticles = true
                };

                extinguishableFire.elements[i + 1] = element;
            }

            var child = new GameObject("FireHolder");
            child.transform.SetParent(instance.transform, false);
            
            var fire = child.EnsureComponent<Fire>();
            fire.livemixin = mixin;
            fire.fireFX = extinguishableFire;
            fire.fireGrowRate = 1;
            fire.minScale = Vector3.one;
            
            var sphereCollider = fire.gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.5f;
            sphereCollider.isTrigger = true;
            fire.gameObject.layer = LayerID.Useable;

            var fireSfx = instance.EnsureComponent<FMOD_CustomLoopingEmitter>();
            fireSfx.playOnAwake = true;
            fireSfx.followParent = true;
            fireSfx.asset = Plugin.AudioBundle.LoadAsset<FMODAsset>("FireSfx");
            
            fire.fireSound = fireSfx;
            instance.GetComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;

            var lightningGreen = new Color(0.0963f, 0.3333f, 0.1429f);
            foreach (var renderer in instance.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                renderer.material.SetColor(ShaderPropertyID._Color, lightningGreen);
            }

            instance.GetComponentInChildren<Light>().color = lightningGreen;
            instance.GetComponentInChildren<LightAnimator>().origIntensity = 1f;
            
            foreach (var particleSystem in instance.GetComponentsInChildren<ParticleSystem>())
            {
                var main = particleSystem.main;
                main.simulationSpeed = 0.8f;
            }
        };

        prefab.SetGameObject(cloneTemplate);
        prefab.Register();
    }
}