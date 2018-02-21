using System;
using UnityEngine;

public class ItemPickup : Interactable {

    public Item item;
    public float chanceToDrop;

    private void Update()
    {
        if (isFocus && !hasInteracted)
        {
            //Debug.Log(gameDetails.player.name);
            float distance = Vector3.Distance(player.transform.position, transform.position);

            if (distance <= radius)
            {
                Interact();
            }
            hasInteracted = true;
        }
    }

    public override void Interact()
    {
        base.Interact();

        Pickup();
    }

    private void Pickup()
    {
        Debug.Log("Picking up " + item.name);
        bool wasPickedUp = Inventory.instance.AddItemToBag(item);

        if (wasPickedUp)
        {
            Destroy(gameObject);
        }
        // Add to inventory

    }
}
