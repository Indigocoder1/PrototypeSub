using rail;
using SubLibrary.SubFire;
using System.Collections;
using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

internal class FireSupressionListener : MonoBehaviour
{

    [SerializeField] private SubRoot subRoot;

    private ProtoStairsManager stairsManager;

    private void Start()
    {
        stairsManager = subRoot.transform.gameObject.GetComponentInChildren<ProtoStairsManager>();
    }

    private void TemporaryClose(float delay)
    {
        UWE.CoroutineHost.StartCoroutine(ToggleStairs(delay));
    }

    private IEnumerator ToggleStairs(float delay)
    {
        stairsManager.ToggleStairsActive();
        stairsManager.fireSupressionActive = true;

        yield return new WaitForSeconds(delay);

        stairsManager.fireSupressionActive = false;
        stairsManager.ToggleStairsActive();
    }

}
