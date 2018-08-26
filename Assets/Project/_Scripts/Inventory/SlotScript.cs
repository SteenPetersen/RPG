using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotScript : MonoBehaviour, IPointerDownHandler, IClickable, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// A stack for all the items in this slot
    /// </summary>
    private ObservableStack<Item> items = new ObservableStack<Item>();

    [SerializeField]
    private Image icon;

    [SerializeField]
    private Text stackSize;

    /// <summary>
    /// reference to the Bag that this slot is associated with
    /// </summary>
    public BagScript MyBag { get; set; }

    private void Awake()
    {
        MyItems.OnPop += new UpdateStackEvent(UpdateSlot);
        MyItems.OnPush += new UpdateStackEvent(UpdateSlot);
        MyItems.OnClear += new UpdateStackEvent(UpdateSlot);

    }

    /// <summary>
    /// Check if slot is empty
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            return MyItems.Count == 0;
        }
    }

    public bool IsFull
    {
        get
        {
            if (IsEmpty)
            {
                return false;
            }
            else if (MyCount < MyItem.MyStackSize)
            {
                return false;
            }

            return true;
        }
    }

    public Item MyItem
    {
        get
        {
            if (!IsEmpty)
            {
                return MyItems.Peek();
            }

            return null;

        }

    }

    public Image MyIcon
    {
        get
        {
            return icon;
        }

        set
        {
            icon = value;
        }
    }

    public int MyCount
    {
        get { return MyItems.Count; }
    }

    public Text MyStackText
    {
        get
        {
            return stackSize;
        }
    }

    public ObservableStack<Item> MyItems
    {
        get
        {
            return items;
        }
    }


    /// <summary>
    /// Adds an item to the items stacks
    /// </summary>
    /// <param name="item">Item to be added to the stack of items</param>
    /// <returns></returns>
    public bool AddItem(Item item)
    {
        MyItems.Push(item);
        icon.sprite = item.icon;
        icon.color = Color.white;
        item.MySlot = this;
        return true;
    }

    /// <summary>
    /// Places a stack of items in a new slot of the bags or tries to stack them if they are of the same type
    /// </summary>
    /// <param name="newItems"></param>
    /// <returns></returns>
    public bool AddItems(ObservableStack<Item> newItems)
    {
        // if the slot is empty or if the slot has items of the same type
        if (IsEmpty || newItems.Peek().GetType() == MyItem.GetType())
        {
            // how many items do I have in my hand?
            int count = newItems.Count;

            // for each of the items I have in my hand
            for (int i = 0; i < count; i++)
            {
                // if the stack is full
                if (IsFull)
                {
                    // then stop trying to add items to it
                    return false;
                }

                // if its not full then add the current iteration of the items in my hand
                AddItem(newItems.Pop());
            }

            // we managed to get through all the items and we have no more items in our hand
            return true;

        }

        // this slot is not empty or the things there are not of the same type so the function cannot do anything
        return false;
    }

    /// <summary>
    /// Removes the item that is on top of the stack
    /// </summary>
    public void RemoveItem(Item item)
    {
        if (!IsEmpty)
        {
            InventoryScript.instance.OnItemCountChanged(MyItems.Pop());
        }
    }

    public void Clear()
    {
        if (MyItems.Count > 0)
        {
            InventoryScript.instance.OnItemCountChanged(MyItems.Pop());
            MyItems.Clear();
        }
    }

    /// <summary>
    /// Determines what happens when the slot is right or left clicked
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // If I didnt pick anything up from another slot
            if (InventoryScript.instance.FromSlot == null && !IsEmpty)
            {
                if (HandScript.instance.MyMoveable != null && HandScript.instance.MyMoveable is Bag)
                {
                    if (MyItem is Bag)
                    {
                        InventoryScript.instance.SwapBags(HandScript.instance.MyMoveable as Bag, MyItem as Bag);
                    }
                }
                else
                {
                    HandScript.instance.TakeMoveable(MyItem as IMoveable);
                    InventoryScript.instance.FromSlot = this;
                }

            }
            // if I didnt pick anything up from another slot
            else if (InventoryScript.instance.FromSlot == null && IsEmpty && (HandScript.instance.MyMoveable is Bag))
            {
                // dequips a bag from the inventory
                Bag bag = (Bag)HandScript.instance.MyMoveable;

                // if your not trying top place the bag inside itself
                if (bag.MyBagScript != MyBag && InventoryScript.instance.MyEmptySlotCount - bag.Slots > 0)
                {
                    AddItem(bag);
                    bag.MyBagButton.RemoveBag();
                    HandScript.instance.Drop();
                }

            }
            // I have picked something up from somewhere else
            else if (InventoryScript.instance.FromSlot != null)
            {
                if (PutItemBack() || MergeItems(InventoryScript.instance.FromSlot) || SwapItems(InventoryScript.instance.FromSlot) || AddItems(InventoryScript.instance.FromSlot.MyItems))
                {
                    HandScript.instance.Drop();
                    InventoryScript.instance.FromSlot = null;
                }
            }
            // I have taken an item that was previously equipped
            else if (InventoryScript.instance.FromEqippedSlot != null && IsEmpty)
            {
                if (AddItem(InventoryScript.instance.FromEqippedSlot.MyEquipment))
                {
                    HandScript.instance.Drop();
                    InventoryScript.instance.FromEqippedSlot.DragClearSlot();
                    InventoryScript.instance.FromEqippedSlot.MyIcon.color = Color.white;
                    InventoryScript.instance.FromEqippedSlot = null;
                }
            }

        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // if the vendor window is currently open And your done with tutorial (else player can sell needed items to the vendors)
            if (VendorManager.instance.vendorWindowOpen && !IsEmpty && ExperienceManager.MyLevel > 1)
            {
                // sell this item
                VendorManager.instance.Sell(MyItem.sellValue, MyItem);
                RemoveItem(MyItem);
                return;
            }

            else if (VendorManager.instance.vendorWindowOpen && !IsEmpty && ExperienceManager.MyLevel <= 1)
            {
                SpeechBubbleManager.instance.FetchBubble(VendorWindow.instance.MyVendor.MySpeechLocation, "Come back when you've got" + "\n" + "a little more experience");
            }


            UseItem();
        }
    }

    /// <summary>
    /// Attempts to use an item if the slot is not empty
    /// </summary>
    public void UseItem()
    {
        if (!IsEmpty)
        {
            MyItem.Use();
        }
    }

    /// <summary>
    /// Checks if items are of the same type and stackable, if so items will stack to predetermined number
    /// </summary>
    /// <param name="item">Item that should attempt to stack</param>
    /// <returns>true if it can stack false if it cannot</returns>
    public bool StackItem(Item item)
    {
        if (!IsEmpty)
        {
            if (item.name == MyItem.name)
            {
                if (MyItems.Count < MyItem.MyStackSize)
                {
                    MyItems.Push(item);
                    item.MySlot = this;
                    return true;
                }
            }
        }

        return false;
    }


    private bool PutItemBack()
    {
        if (InventoryScript.instance.FromSlot == this)
        {
            InventoryScript.instance.FromSlot.MyIcon.color = Color.white;
            return true;
        }

        return false;
    }

    private bool SwapItems(SlotScript from)
    {
        if (IsEmpty)
        {
            return false;
        }
        // if the item im moving is different from the item(s) in this slot
        // or if the count of items in my hand plus the items in this slot is larger than the total stacksize allowed 
        if (from.MyItem.GetType() != MyItem.GetType() || from.MyCount+MyCount > MyItem.MyStackSize)
        {
            // copy all the items we need to swap from A
            ObservableStack<Item> tmpFrom = new ObservableStack<Item>(from.MyItems);

            // clear slot A
            from.MyItems.Clear();
            // take all items from slot B and copy them into A
            from.AddItems(MyItems);

            // Clear slot B
            MyItems.Clear();
            // Adding the items we originally copied into this slot
            AddItems(tmpFrom);

            return true;

        }

        return false;
    }

    /// <summary>
    /// Merges a stack of items to allow player to organise his/her bags
    /// 11.10
    /// </summary>
    /// <returns></returns>
    private bool MergeItems(SlotScript from)
    {
        if (IsEmpty)
        {
            return false;
        }
        if (from.MyItem.GetType() == MyItem.GetType() && !IsFull)
        {
            // how many free slots are there
            int free = MyItem.MyStackSize - MyCount;

            for (int i = 0; i < free; i++)
            {
                if (from.MyCount > 0)
                {
                    AddItem(from.MyItems.Pop());
                }
            }

            return true;

        }
        return false;
    }

    private void UpdateSlot()
    {
        UiManager.instance.UpdateStackSize(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // show tooltip
        if (!IsEmpty)
        {
            UiManager.instance.ShowToolTip(transform.position, MyItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // hide tooltip
        UiManager.instance.HideToolTip();
    }
}