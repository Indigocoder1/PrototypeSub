using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

public class DisableWithFins : MonoBehaviour
{
    [SerializeField] private ProtoFinsManager finsManager;
    [SerializeField] private int finsIndex;

    private void Start()
    {
        finsManager.OnFinCountChanged += UpdateActive;
        UpdateActive();
    }

    private void UpdateActive()
    {
        gameObject.SetActive(finsIndex + 1 <= finsManager.GetInstalledFinCount());
    }
}