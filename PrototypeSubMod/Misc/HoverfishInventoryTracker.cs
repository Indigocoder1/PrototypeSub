using PrototypeSubMod.Facilities;
using PrototypeSubMod.Prefabs;
using Story;
using System;
using System.Collections;
using UnityEngine;

public class HoverfishInventoryTracker : MonoBehaviour
{

	private float timeHeld = 0f;
	private float timeToHold = 300f; // 5 minutes
	private bool isPickedUp = false;


    private void Start()
	{
        if (KnownTech.Contains(HoverfishPlush.prefabInfo.TechType)) return;

        Pickupable pickupable = this.GetComponent<Pickupable>();

		if (pickupable != null)
		{
			pickupable.pickedUpEvent.AddHandler(this, OnPickedUp);

			pickupable.droppedEvent.AddHandler(this, OnDropped);
        }
    }


	private void OnDropped(Pickupable pickupable)
	{
		isPickedUp = false;
    }
    private void OnPickedUp(Pickupable pickupable)
	{
		isPickedUp = true;
        UWE.CoroutineHost.StartCoroutine(CountHeldTime());
    }

    private IEnumerator CountHeldTime()
    {
        while (timeHeld < timeToHold && isPickedUp)
        {
            timeHeld += 1;
            yield return new WaitForSeconds(1f);
        }
        if (!isPickedUp) yield break;

        // Goal completed
        StoryGoalManager.main.OnGoalComplete("OnHoverfishPlushUnlocked");
        Destroy(this);
    }

    private void OnDestroy()
	{
		isPickedUp = false;

        Pickupable pickupable = this.GetComponent<Pickupable>();
        pickupable?.pickedUpEvent.RemoveHandler(this, OnPickedUp);
        pickupable?.pickedUpEvent.RemoveHandler(this, OnDropped);
    }

}
