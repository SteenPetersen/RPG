using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickup : Interactable {

    [SerializeField] Key key;

    public Key MyKey
    {
        get
        {
            return key;
        }

        set
        {
            key = value;
        }
    }

    [SerializeField] Transform tooltipPosition;
    [SerializeField] bool mouseOver;

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
            if (key._BossKey)
            {
                DungeonManager.instance._PlayerHasBossKey = true;
            }

            SoundManager.instance.PlayInventorySound(key.typeOfEquipment + "_pickup");
            bool wasPickedUp = InventoryScript.instance.AddItem(Instantiate(MyKey));

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

    public override void OnMouseOver()
    {
        base.OnMouseOver();

        if (!mouseOver)
        {
            mouseOver = true;
        }
    }

    public override void OnMouseExit()
    {
        base.OnMouseExit();

        mouseOver = false;
        UiManager.instance.HideToolTip();

    }

    void ToolTipDisplay()
    {
        //If your mouse hovers over the GameObject with the script attached, output this message
        if (tooltipPosition != null)
        {
            UiManager.instance.ShowToolTip(Camera.main.WorldToScreenPoint(tooltipPosition.position), MyKey, true, 0.7f);
            return;
        }
        UiManager.instance.ShowToolTip(Camera.main.WorldToScreenPoint(transform.position), MyKey, true, 0.7f);
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
