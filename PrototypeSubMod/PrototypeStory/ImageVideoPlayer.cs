using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.PrototypeStory;

public class ImageVideoPlayer : MonoBehaviour
{
    [SerializeField] private Image videoImage;
    [SerializeField] private Sprite[] images;
    [SerializeField] private int fps;

    private float timer;
    private float secondsBetweenFrames;
    private int imageIndex;

    private void Start()
    {
        secondsBetweenFrames = 1f / fps;
        videoImage.sprite = images[imageIndex];
    }

    private void Update()
    {
        if (imageIndex >= images.Length) return;
        
        timer += Time.deltaTime;
        var targetImageIndex = Mathf.FloorToInt(timer / secondsBetweenFrames);
        
        if (targetImageIndex == imageIndex) return;
        
        imageIndex = targetImageIndex;
        if (imageIndex >= images.Length) return;
        
        videoImage.sprite = images[imageIndex];
    }
}