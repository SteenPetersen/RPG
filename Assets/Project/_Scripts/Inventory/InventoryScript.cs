using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ItemCountChanged(Item item);

public class InventoryScript : MonoBehaviour {

    public event ItemCountChanged itemCountChangedEvent;

    public static InventoryScript instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }


        Bag bag = (Bag)Instantiate(itemListForDebugging[0]);
        bag.Initialize(16);
        bag.Use();
        bag.MyBagScript.OpenClose();
    }

    /// <summary>
    /// Slot the items are coming from when being dragged around
    /// </summary>
    private SlotScript fromSlot;

    private EquipedItemSlot fromEqippedSlot;

    [SerializeField]
    private List<Bag> bags = new List<Bag>();

    [SerializeField]
    private BagButton[] bagButtons;

    [SerializeField]
    private Item[] itemListForDebugging; // for debugging delete it later

    public bool CanAddBag
    {
        get { return bags.Count < 5; }
    }

    /// <summary>
    /// Counts the overall amount of empty slots for all bags
    /// </summary>
    public int MyEmptySlotCount
    {
        get
        {
            int count = 0;

            foreach (Bag bag in bags)
            {
                count += bag.MyBagScript.MyEmptySlotCount;
            }
            return count;
        }
    }

    public int MyTotalSlotCount
    {
        get
        {
            int count = 0;

            foreach (Bag bag in bags)
            {
                // run through all the bags in the invcentory and add all its slots to the count
                count += bag.MyBagScript.MySlots.Count;
            }

            return count;
        }
    }

    /// <summary>
    /// Return the amount of slots that have things in them
    /// </summary>
    public int MyFullSlotCount
    {
        get
        {
            return MyTotalSlotCount - MyEmptySlotCount;
        }
    }

    public List<Bag> MyBags
    {
        get
        {
            return bags;
        }
    }

    public BagButton[] MyBagButtons
    {
        get
        {
            return bagButtons;
        }
    }

    public SlotScript FromSlot
    {
        get
        {
            return fromSlot;
        }

        set
        {
            fromSlot = value;

            if (value != null)
            {
                fromSlot.MyIcon.color = Color.gray;
            }
        }
    }

    public EquipedItemSlot FromEqippedSlot
    {
        get
        {
            return fromEqippedSlot;
        }

        set
        {
            fromEqippedSlot = value;

            if (value != null)
            {
                fromEqippedSlot.MyIcon.color = Color.gray;
            }
        }
    }

    public Item[] MyItems
    {
        get
        {
            return itemListForDebugging;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Bag bag = (Bag)Instantiate(itemListForDebugging[0]);
            bag.Initialize(16);
            bag.Use();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Bag bag = (Bag)Instantiate(itemListForDebugging[0]);
            bag.Initialize(16);
            AddItem(bag);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            SmallHealthPotion potion = (SmallHealthPotion)Instantiate(itemListForDebugging[1]);
            AddItem(potion);
        }
    }

    /// <summary>
    /// Equips a bag in th inventory
    /// </summary>
    /// <param name="bag"></param>
    public void AddBag(Bag bag)
    {
        foreach (BagButton bagButton in bagButtons)
        {
            if (bagButton.MyBag == null)
            {
                bagButton.MyBag = bag;
                bags.Add(bag);
                bag.MyBagButton = bagButton;
                break;
            }
        }
    }

    public void AddBag(Bag bag, BagButton bagButton)
    {
        bags.Add(bag);
        bagButton.MyBag = bag;
    }

    /// <summary>
    /// Removes the bag by destroying it
    /// </summary>
    /// <param name="bag"></param>
    public void RemoveBag(Bag bag)
    {
        bags.Remove(bag);
        Destroy(bag.MyBagScript.gameObject);
    }

    /// <summary>
    /// Swaps bags on the bag bar
    /// </summary>
    /// <param name="oldBag">The old bag that was in the position</param>
    /// <param name="newBag">The new bag that will replace the old bag</param>
    public void SwapBags(Bag oldBag, Bag newBag)
    {
        // the amount of slots that will be available to use after the swap
        int newSlotCount = (MyTotalSlotCount - oldBag.Slots) + newBag.Slots;

        // are there at least the same amount of slots available after the swap?
        if (newSlotCount - MyFullSlotCount >= 0)
        {
            // Do swap
            List<Item> itemsThatWereInTheBag = oldBag.MyBagScript.GetItems();

            RemoveBag(oldBag);

            // assign a bagbutton so we use the overload addbag to make sure bag gets placed in the correct slot on the bagbar.
            newBag.MyBagButton = oldBag.MyBagButton;

            newBag.Use();

            foreach (Item item in itemsThatWereInTheBag)
            {
                // to ensure there is duplication of bags
                if (item != newBag)
                {
                    AddItem(item);
                }
            }

            AddItem(oldBag);

            HandScript.instance.Drop();

            instance.fromSlot = null;
        }
    }

    /// <summary>
    /// Checks to see if item can be placed in stackable form anywhere in inventory, if it cannot places it in an empty slot
    /// </summary>
    /// <param name="item">Item to add</param>
    public bool AddItem(Item item)
    {
        if (MyEmptySlotCount != 0)
        {
            if (item.MyStackSize > 0)
            {
                if (PlaceInStack(item))
                {
                    return true;
                }
            }

            PlaceInEmpty(item);
            return true;
        }
        else
        {
            if (item.MyStackSize > 0)
            {
                if (PlaceInStack(item))
                {
                    return true;
                }
            }
        }

        return false;

    }


    /// <summary>
    /// Places an item in an empty slot/bag
    /// </summary>
    /// <param name="item">Item to add to the bags</param>
    private void PlaceInEmpty(Item item)
    {
        foreach (Bag bag in bags)
        {
            if (bag.MyBagScript.AddItem(item))
            {
                OnItemCountChanged(item);
                return;
            }
        }
    }

    /// <summary>
    /// checks if it is possible to add the item to the bag
    /// </summary>
    /// <param name="item">Item we are trying to return</param>
    /// <returns>A boolean of wether or not item can be added</returns>
    private bool PlaceInStack(Item item)
    {
        // for each bag
        foreach (Bag bag in bags)
        {
            // check all slots in that bag
            foreach (SlotScript slots in bag.MyBagScript.MySlots)
            {
                if (slots.StackItem(item))
                {
                    OnItemCountChanged(item);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Opens and closes Bags
    /// </summary>
    public void OpenClose()
    {
        bool closedBag = bags.Find(x => !x.MyBagScript.isOpen);

        // if closedbag is true then open all closed bags
        foreach (Bag bag in bags)
        {
            if (bag.MyBagScript.isOpen != closedBag)
            {
                bag.MyBagScript.OpenClose();
            }
        }
    }

    public Stack<IUseable> GetUseables(IUseable type, Item item)
    {
        Stack<IUseable> useables = new Stack<IUseable>();

        foreach (Bag bag in bags)
        {
            foreach (SlotScript slot in bag.MyBagScript.MySlots)
            {
                if (!slot.IsEmpty && slot.MyItem.GetType() == type.GetType() && slot.MyItem.name == item.name)
                {
                    foreach (Item currentItem in slot.MyItems)
                    {
                        useables.Push(currentItem as IUseable);
                    }
                }
            }
        }

        return useables;
    }

    /// <summary>
    /// returns a list of all slots currently available
    /// </summary>
    /// <returns></returns>
    public List<SlotScript> GetAllSlots()
    {
        List<SlotScript> slots = new List<SlotScript>();

        foreach (Bag bag in bags)
        {
            foreach (SlotScript slot in bag.MyBagScript.MySlots)
            {
                slots.Add(slot);
            }
        }

        return slots;
    }

    public void OnItemCountChanged(Item item)
    {
        if (itemCountChangedEvent != null)
        {
            itemCountChangedEvent.Invoke(item);
        }
    }



}
