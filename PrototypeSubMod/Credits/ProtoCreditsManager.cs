using System.Collections;
using PrototypeSubMod.PrototypeStory.TransmissionDevice;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PrototypeSubMod.Credits;

internal class ProtoCreditsManager : MonoBehaviour
{
    public static bool QueueTransmissionEnding;
    
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform creditsTextRect;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private RectTransform titleImage;
    [SerializeField] private FMOD_CustomEmitter normalMusic;
    [SerializeField] private float creditsLength;
    
    [Header("Transmission Ending")]
    [SerializeField] private GameObject starsCanvas;
    [SerializeField] private float transmissionCreditsLength;
    [SerializeField] private float waitAfterCredits;
    [SerializeField] private float timePlayTransmissionVoiceline;
    [SerializeField] private FMOD_CustomEmitter transmissionMusic;
    [SerializeField] private FMOD_CustomEmitter transmissionVoiceline;

    private float UsedCreditsLength => QueueTransmissionEnding ? transmissionCreditsLength : creditsLength;

    private float creditsSpeed;
    private float currentCreditsLength;
    private bool loadedMainMenu;
    private bool initialized;

    private float maskYHeight;
    private float textYHeight;
    private float yOffset;
    
    private void Start()
    {
        TransmissionDeviceManager.ResetStaticVariables();
        
        if (QueueTransmissionEnding)
        {
            transmissionMusic.Play();
        }
        else
        {
            normalMusic.Play();
        }
        
        starsCanvas.SetActive(QueueTransmissionEnding);
        
        creditsText.text = Language.main.Get("ProtoCreditsText");
        Canvas.ForceUpdateCanvases();
        
        maskYHeight = canvas.GetComponent<RectTransform>().rect.height;
        textYHeight = creditsTextRect.rect.height;
        var imageHeight = titleImage.rect.height;
        yOffset = -(maskYHeight / 2) - (textYHeight / 2) - (imageHeight / 2);
        creditsTextRect.localPosition = new Vector3(0, yOffset, 0);
        
        creditsSpeed = (textYHeight + maskYHeight + imageHeight) / UsedCreditsLength;
        
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        var targetLength = UsedCreditsLength + (QueueTransmissionEnding ? waitAfterCredits : 0);
        if (currentCreditsLength < targetLength)
        {
            currentCreditsLength += Time.deltaTime;
            creditsTextRect.localPosition += new Vector3(0, creditsSpeed * Time.deltaTime, 0);
        }
        else if (!loadedMainMenu)
        {
            StartCoroutine(LoadMainMenu());
            loadedMainMenu = true;
        }

        if (currentCreditsLength >= timePlayTransmissionVoiceline && QueueTransmissionEnding &&
            !transmissionVoiceline.playing)
        {
            transmissionVoiceline.Play();
        }
    }

    private IEnumerator LoadMainMenu()
    {
        QueueTransmissionEnding = false;
        yield return new WaitForSeconds(1);
        SceneCleaner.Open();
    }
}
