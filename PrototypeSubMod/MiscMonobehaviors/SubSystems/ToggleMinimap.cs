using System.Collections;
using PrototypeSubMod.UI.AbilitySelection;
using PrototypeSubMod.UI.ActivatedAbilities;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

internal class ToggleMinimap : MonoBehaviour, IAbilityIcon
{
    [SerializeField] private Sprite minimapSprite;
    [SerializeField] private FMOD_CustomEmitter nearfieldSFX;
    [SerializeField] private GameObject positionDisplay;
    [SerializeField] private int maxSpawnWaitFrames = 10;

    private int frameCount;
    private MiniWorld miniWorld;

    private void Start()
    {
        UWE.CoroutineHost.StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        yield return new WaitForEndOfFrame();
        
        positionDisplay.SetActive(false);

        while (frameCount < maxSpawnWaitFrames)
        {
            yield return new WaitForEndOfFrame();

            var world = gameObject.GetComponentInChildren<MiniWorld>();
            if (world)
            {
                miniWorld = world;
                miniWorld.active = false;
                yield break;
            }

            frameCount++;
        }

        Plugin.Logger.LogError($"Mini world not found as a child of {gameObject} after {maxSpawnWaitFrames} frames");
    }

    public void ToggleMap()
    {
        if (!miniWorld) return;

        miniWorld.ToggleMap();
        positionDisplay.SetActive(!positionDisplay.activeSelf);

        if (miniWorld.active)
        {
            nearfieldSFX.Play();
        }
        else
        {
            nearfieldSFX.Stop();
        }
    }

    public void ToggleMap(bool active)
    {
        if (!miniWorld) return;

        miniWorld.active = active;
        positionDisplay.SetActive(active);
        nearfieldSFX.Stop();
    }
    
    // Called by BroadcastMessage in SubRoot.OnPlayerExited
    public void SaveEngineStateAndPowerDown()
    {
        ToggleMap(false);
        GetComponentInParent<SubRoot>().GetComponentInChildren<TetherManager>(true)
            .UpdateIcon(this);
    }

    public bool OnActivated()
    {
        //ToggleMap();
        var sonarVFX = Camera.main.gameObject.GetComponent<ProtoSonarVFXManager>();
        sonarVFX.ToggleActivated();

        if (sonarVFX.activated)
        {
            nearfieldSFX.Play();
        }
        else
        {
            nearfieldSFX.Stop();
        }
        
        return true;
    }

    public void OnSelectedChanged(bool changed) { }

    public bool GetActive()
    {
        return miniWorld.active;
    }

    public bool GetCanActivate() => true;
    public bool GetShouldShow() => true;
    public Sprite GetSprite() => minimapSprite;
    public TechType GetTechType() => TechType.None;
}
