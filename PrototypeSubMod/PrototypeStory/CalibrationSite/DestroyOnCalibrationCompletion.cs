using UnityEngine;

namespace PrototypeSubMod.PrototypeStory.CalibrationSite;

public class DestroyOnCalibrationCompletion : MonoBehaviour
{
    private void Start()
    {
        CalibrationRunManager.OnCalibrationCompleted += Destroy;
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        CalibrationRunManager.OnCalibrationCompleted -= Destroy;
    }
}