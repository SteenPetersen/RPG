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
                //Debug.Log("Setting sprite to full");

                GetComponent<Image>().sprite = full;
            }
            else
            {
                //Debug.Log("Setting sprite to Empty");

                GetComponent<Image>().sprite = empty;
            }
            bag = value;
        }
    }

    /// <summary>
    /// determines what happens when the bagbutton is clicked
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // if you grabbed the bag from somewhere in your inventory
            if (InventoryScript.instance.FromSlot != null && HandScript.instance.MyMoveable != null && HandScript.instance.MyMoveable is Bag)
            {
                // if we have a bag equipped already
                if (MyBag != null)
                {
                    InventoryScript.instance.SwapBags(MyBag, HandScript.instance.MyMoveable as Bag);
                }
                else
                {
                    // make a new replica bag
                    Bag tmp = (Bag)HandScript.instance.MyMoveable;

                    // set the replica bag to be allocated to this slot
                    tmp.MyBagButton = this;

                    tmp.Use();

                    MyBag = tmp;

                    HandScript.instance.Drop();

                    InventoryScript.instance.FromSlot = null;
                }
            }

            // if you are just changing the bags positions on the bagbar
            if (InventoryScript.instance.FromSlot == null && HandScript.instance.MyMoveable != null && HandScript.instance.MyMoveable is Bag)
            {
                // if we have a bag equipped already
                if (MyBag != null)
                {
                    InventoryScript.instance.SwapBags(MyBag, HandScript.instance.MyMoveable as Bag);
                }
                else
                {
                    // make a new replica bag
                    Bag tmp = (Bag)HandScript.instance.MyMoveable;

                    if (tmp.MyBagButton.MyBag.MyBagScript.isOpen)
                    {
                        tmp.MyBagButton.MyBag.MyBagScript.OpenClose();
                    }

                    tmp.MyBagButton.RemoveBag();

                    tmp.MyBagButton = this;

                    tmp.Use();

                    MyBag = tmp;

                    HandScript.instance.Drop();

                    InventoryScript.instance.FromSlot = null;
                }
            }

            else if (Input.GetKey(KeyCode.LeftShift))
            {
                if (HandScript.instance.MyMoveable == null)
                {
                    HandScript.instance.TakeMoveable(MyBag);
                }
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
