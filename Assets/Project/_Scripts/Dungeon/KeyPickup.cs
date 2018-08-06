using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickup : Interactable {

    public override void Interact()
    {
        base.Interact();

        Pickup();
    }

    private void Pickup()
    {
        if (!hasInteracted)
        {
            DungeonManager.Instance.playerHasBossKey = true;
            Destroy(gameObject);
        }
    }
}
