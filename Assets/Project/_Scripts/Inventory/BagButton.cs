using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BagButton : MonoBehaviour, IPointerClickHandler {

    private Bag bag;

    [SerializeField]
    private Sprite full, empty = null;

    public Bag MyBag
    {
        get
        {
            return bag;
        }

        // this propery sets the value of the sprite depending on the value
        set
        {
            if (value != null)
            {
                Debug.Log("Setting sprite to full");

                GetComponent<Image>().sprite = full;
            }
            else
            {
                Debug.Log("Setting sprite to Empty");

                GetComponent<Image>().sprite = empty;
            }
            bag = value;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (InventoryScript.instance.FromSlot != null && HandScript.instance.MyMoveable != null && HandScript.instance.MyMoveable is Bag)
            {
                // if we have a bag equipped already
                if (MyBag != null)
                {
                    InventoryScript.instance.SwapBags(MyBag, HandScript.instance.MyMoveable as Bag);
                }
                else
                {
                    Bag tmp = (Bag)HandScript.instance.MyMoveable;
                    tmp.MyBagButton = this; // make it be equipped on a specific slot in the bagbar
                    tmp.Use();
                    MyBag = tmp;
                    HandScript.instance.Drop();
                    InventoryScript.instance.FromSlot = null;
                }
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                HandScript.instance.TakeMoveable(MyBag);
            }

        }
        else if (bag != null)
        {
            bag.MyBagScript.OpenClose();
        }
    }

    /// <summary>
    /// removes a bag from the bagbar nulls the button and sets the MyBag to null, then places all its items inside the other bags
    /// </summary>
    public void RemoveBag()
    {
        InventoryScript.instance.RemoveBag(MyBag);
        MyBag.MyBagButton = null;

        // run through each item in the list you get back from GetItems() and add those to the inventory
        foreach (Item item in MyBag.MyBagScript.GetItems())
        {
            InventoryScript.instance.AddItem(item);
        }

        MyBag = null;
    }
}
