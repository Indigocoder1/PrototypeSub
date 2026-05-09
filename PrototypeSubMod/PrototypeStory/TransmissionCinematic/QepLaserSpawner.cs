using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.TransmissionCinematic;

public class QepLaserSpawner : MonoBehaviour
{
    [SerializeField] private Transform warmupParent;
    [SerializeField] private Transform beamParent;
    [SerializeField] private Transform muzzleParent;
    [SerializeField] private Color muzzleGlowColor = Color.white;

    private ParticleSystem warmupVFX;
    private ParticleSystem muzzleVFX;
    private GameObject laserVFX;
    
    private void Start()
    {
        warmupVFX = Instantiate(VFXSunbeam.main.warmupPrefab, warmupParent).GetComponent<ParticleSystem>();
        muzzleVFX = Instantiate(VFXSunbeam.main.muzzlePrefab, muzzleParent).GetComponent<ParticleSystem>();
        laserVFX = Instantiate(VFXSunbeam.main.beamPrefab, beamParent);

        muzzleVFX.transform.Find("xGlow").GetComponent<Renderer>().material.color = muzzleGlowColor;
        var muzzleBeam = muzzleVFX.transform.Find("xBeam").GetComponent<ParticleSystem>();
        var main = muzzleBeam.main;
        main.loop = false;
        
        UWE.Utils.ZeroTransform(warmupVFX.gameObject);
        UWE.Utils.ZeroTransform(muzzleVFX.gameObject);
        UWE.Utils.ZeroTransform(laserVFX);

        Destroy(muzzleVFX.GetComponent<VFXDestroyAfterSeconds>());
        
        warmupVFX.gameObject.SetActive(false);
        muzzleVFX.gameObject.SetActive(false);
        laserVFX.SetActive(false);
    }

    public void PlayWarmupVFX()
    {
        warmupVFX.gameObject.SetActive(true);
        warmupVFX.Play();
    }
    
    public void StopWarmupVFX()
    {
        warmupVFX.Stop();
    }

    public void PlayMuzzleVFX()
    {
        muzzleVFX.gameObject.SetActive(true);
        muzzleVFX.Play();
    }
    
    public void PlayLaserVFX()
    {
        laserVFX.gameObject.SetActive(true);
    }
}