﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemPickup : Interactable
{
    public Item item;
    public float chanceToDrop;
    [SerializeField] Transform tooltipPosition;
    PlayerController player;

    bool showBagsFullText = true;

    [SerializeField] bool mouseOver;
    [SerializeField] bool displayingTooltip;

    void Start()
    {
        player = PlayerController.instance;
    }

    public override void Interact()
    {
        base.Interact();

        Pickup();
    }

    void Update()
    {
        if (mouseOver)
        {
            ToolTipDisplay();
        }
    }

    private void Pickup()
    {
        if (!hasInteracted)
        {
            Debug.Log(item.typeOfEquipment + "_pickup");
            SoundManager.instance.PlayInventorySound(item.typeOfEquipment + "_pickup");
            bool wasPickedUp = InventoryScript.instance.AddItem(Instantiate(item));

            if (wasPickedUp)
            {
                Destroy(gameObject);

                var text = CombatTextManager.instance.FetchText(transform.position);
                var textScript = text.GetComponent<CombatText>();
                textScript.White(gameObject.name, transform.position);

                UiManager.instance.HideToolTip();
                mouseOver = false;
                hasInteracted = true;
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

    void OnMouseEnter()
    {
        if (!mouseOver)
        {
            player.mouseOverItem = true;
            mouseOver = true;

            if (!displayingTooltip)
            {
                displayingTooltip = true;
            }
        }
    }

    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        //Debug.Log("Mouse is no longer on GameObject.");
        if (player.mouseOverItem)
        {
            player.mouseOverItem = false;
        }

        displayingTooltip = false;

        UiManager.instance.HideToolTip();
        mouseOver = false;
    }

    void ToolTipDisplay()
    {
        //If your mouse hovers over the GameObject with the script attached, output this message
        if (tooltipPosition != null)
        {
            UiManager.instance.ShowToolTip(Camera.main.WorldToScreenPoint(tooltipPosition.position), item, true, 0.7f);
            return;
        }
        UiManager.instance.ShowToolTip(Camera.main.WorldToScreenPoint(transform.position), item, true, 0.7f);
    }
}
