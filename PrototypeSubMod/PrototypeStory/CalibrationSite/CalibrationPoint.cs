using PrototypeSubMod.Puzzles.BearingPuzzle;
using UnityEngine;
using UnityEngine.UI;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class CalibrationPoint : MonoBehaviour
{
    [SerializeField] private Transform referenceSymbolParent;
    [SerializeField] private AnimationCurve alphaOverDistance;
    [SerializeField] private float maxUpdateDistance;
    
    private Image bearingImage;
    
    private void LateUpdate()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null) return;
        if (bearingImage == null) return;
        
        var distToCamera = Vector3.Distance(mainCamera.transform.position, transform.position);
        
        if (distToCamera > maxUpdateDistance) return;

        UpdateAlpha(distToCamera);

        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position, Vector3.up);
    }

    private void UpdateAlpha(float distToPlayer)
    {
        var color = bearingImage.color;
        color.a = alphaOverDistance.Evaluate(distToPlayer);
        bearingImage.color = color;
    }

    public void SetBearingReference(BearingReferenceSymbol bearingReferenceSymbol)
    {
        foreach (Transform child in referenceSymbolParent)
        {
            Destroy(child.gameObject);
        }

        var symbolObject = bearingReferenceSymbol.CreateSymbolObject();
        symbolObject.transform.SetParent(referenceSymbolParent, false);
        bearingImage = symbolObject.transform.Find("Image").GetComponent<Image>();
        
        var distToCamera = Vector3.Distance(Camera.main.transform.position, transform.position);
        UpdateAlpha(distToCamera);
    }
}