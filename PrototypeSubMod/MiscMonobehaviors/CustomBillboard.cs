using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors;

public class CustomBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(-targetCamera.transform.forward);
    }
}