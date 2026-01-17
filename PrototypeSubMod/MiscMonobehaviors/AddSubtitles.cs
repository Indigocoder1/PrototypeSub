using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class AddSubtitles : MonoBehaviour
{
    [SerializeField] private string subtitlesKey;

    public void PlaySubtitles()
    {
        Subtitles.Add(subtitlesKey);
    }
}