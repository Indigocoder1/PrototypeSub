using UnityEngine;

namespace PrototypeSubMod.MiscMonobehaviors.SubSystems;

[RequireComponent(typeof(Collider))]
internal class EntryWaterGate : MonoBehaviour
{
    [SerializeField] private SubRoot subRoot;
    [SerializeField] private bool setInSub;

    private void OnTriggerEnter(Collider col)
    {
        if (col.isTrigger) return;
        
        bool isPlayerCol = col.gameObject.Equals(Player.main.gameObject);

        if (!isPlayerCol) return;

        Player.main.SetCurrentSub(setInSub ? subRoot : null, true);
        if (setInSub)
        {
            Player.main.precursorOutOfWater = false;
        }
    }
}
