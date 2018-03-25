using System;
using System.Collections;
using UnityEngine;

public class ItemPickup : Interactable {

    public Item item;
    public float chanceToDrop;

    bool showBagsFullText = true;

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
        if (!hasInteracted)
        {

            Debug.Log("Picking up " + item.name);
            SoundManager.instance.PlayInventorySound(gameObject.name + "_pickup");
            bool wasPickedUp = Inventory.instance.AddItemToBag(item);

            if (wasPickedUp)
            {
                Destroy(gameObject);

                var text = CombatTextManager.instance.FetchText(transform.position);
                var textScript = text.GetComponent<CombatText>();
                textScript.White(gameObject.name, transform.position);
                text.SetActive(true);
            }
            else if (!wasPickedUp)
            {
                if (showBagsFullText)
                {
                    StartCoroutine(BagsFull());
                }

            }
        }

        // Add to inventory

    }

    private IEnumerator BagsFull()
    {
        showBagsFullText = false;

        var text = CombatTextManager.instance.FetchText(transform.position);
        var textScript = text.GetComponent<CombatText>();
        textScript.White("Bag Full!", transform.position);
        text.SetActive(true);

        yield return new WaitForSeconds(1);

        showBagsFullText = true;
    }
}
