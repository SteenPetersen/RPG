using System;
using System.Collections;
using UnityEngine;

public class ItemPickup : Interactable {

    public Item item;
    public float chanceToDrop;

    bool showBagsFullText = true;

    public override void Interact()
    {
        base.Interact();

        Pickup();
    }

    private void Pickup()
    {
        if (!hasInteracted)
        {
            SoundManager.instance.PlayInventorySound(gameObject.name + "_pickup");
            bool wasPickedUp = InventoryScript.instance.AddItem(item);

            if (wasPickedUp)
            {
                Destroy(gameObject);

                var text = CombatTextManager.instance.FetchText(transform.position);
                var textScript = text.GetComponent<CombatText>();
                textScript.White(gameObject.name, transform.position);
            }
            else if (!wasPickedUp)
            {
                if (showBagsFullText)
                {
                    StartCoroutine(BagsFull());
                }

            }
        }
    }

    /// <summary>
    /// Let the player know that the bags are full
    /// </summary>
    /// <returns></returns>
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
