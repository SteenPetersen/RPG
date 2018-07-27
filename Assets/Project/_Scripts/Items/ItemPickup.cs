using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemPickup : Interactable
{
    public Item item;
    public float chanceToDrop;

    bool showBagsFullText = true;

    [SerializeField] bool mouseOver;

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

                UiManager.instance.HideToolTip();
                mouseOver = false;
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

    void OnMouseOver()
    {
        if (!mouseOver)
        {
            //If your mouse hovers over the GameObject with the script attached, output this message
            Debug.Log("Mouse is over GameObject.");
            UiManager.instance.ShowToolTip(Camera.main.WorldToScreenPoint(transform.position), item, true, 0.7f);
            mouseOver = true;
        }

    }

    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        Debug.Log("Mouse is no longer on GameObject.");
        UiManager.instance.HideToolTip();
        mouseOver = false;
    }
}
