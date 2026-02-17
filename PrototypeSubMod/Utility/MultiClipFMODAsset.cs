using FMOD;
using Nautilus.Utility;
using UnityEngine;

namespace PrototypeSubMod.Utility;

[CreateAssetMenu(fileName = "CustomFMODAsset", menuName = "Prototype Sub/Create Multi-Clip FMOD Asset")]
public class MultiClipFMODAsset : FMODAsset
{
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(name)) return;
        
        path = name;
    }

    public AudioClip[] audioClips;
    public MODE mode = AudioUtils.StandardSoundModes_3D;
    public float maxDistance3D;
    public float minDistance3D;
    [Tooltip("Leave at -1 to not use fading")]
    public float fadeOutTime = -1;
    public bool randomizePlayOrder;

    [SerializeField] private SoundBus bus;
    [SerializeField] private string customBusPath;

    public string GetBus() => bus switch
    {
        SoundBus.Custom => customBusPath,
        SoundBus.Sfx => "bus:/master/SFX_for_pause/PDA_pause/all/SFX",
        SoundBus.Pda => AudioUtils.BusPaths.PDAVoice,
        SoundBus.VoiceLine => AudioUtils.BusPaths.VoiceOvers,
        SoundBus.Music => AudioUtils.BusPaths.Music,
        SoundBus.UnderwaterCreature => AudioUtils.BusPaths.UnderwaterCreatures,
        SoundBus.SurfaceCreature => AudioUtils.BusPaths.SurfaceCreatures,
        SoundBus.Reverb => AudioUtils.BusPaths.PlayerSFXs,
        _ => string.Empty,
    };
}

internal enum SoundBus
{
    Custom = -1,
    Sfx,
    Pda,
    VoiceLine,
    Music,
    UnderwaterCreature,
    SurfaceCreature,
    Reverb
}