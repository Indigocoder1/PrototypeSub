using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.Credits;

internal class ProtoCreditsManager : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform creditsTextRect;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private RectTransform titleImage;
    [SerializeField] private float creditsLength;
    
    private float creditsSpeed;
    private float currentCreditsLength;
    private bool loadedMainMenu;
    private bool initialized;

    private float maskYHeight;
    private float textYHeight;
    private float yOffset;
    
    private void Start()
    {
        creditsText.text = Language.main.Get("ProtoCreditsText");
        Canvas.ForceUpdateCanvases();
        
        maskYHeight = canvas.GetComponent<RectTransform>().rect.height;
        textYHeight = creditsTextRect.rect.height;
        var imageHeight = titleImage.rect.height;
        yOffset = -(maskYHeight / 2) - (textYHeight / 2) - (imageHeight / 2);
        creditsTextRect.localPosition = new Vector3(0, yOffset, 0);
        
        creditsSpeed = (textYHeight + maskYHeight + imageHeight) / creditsLength;
        
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;
        
        if (currentCreditsLength < creditsLength)
        {
            currentCreditsLength += Time.deltaTime;
            creditsTextRect.localPosition += new Vector3(0, creditsSpeed * Time.deltaTime, 0);
        }
        else if (!loadedMainMenu)
        {
            StartCoroutine(LoadMainMenu());
            loadedMainMenu = true;
        }
    }

    private IEnumerator LoadMainMenu()
    {
        yield return new WaitForSeconds(1);
        SceneCleaner.Open();
    }
}
