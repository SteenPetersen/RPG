using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagScript : MonoBehaviour {

    [SerializeField]
    private GameObject slotPrefab;

    private CanvasGroup canvasGroup;

    private List<SlotScript> slots = new List<SlotScript>();

    public bool isOpen
    {
        get { return canvasGroup.alpha > 0; }
    }

    public List<SlotScript> MySlots
    {
        get
        {
            return slots;
        }
    }

    /// <summary>
    /// Count how many empty slots a bag has, necessary for unequiping bags and not destroying items lost in bags.
    /// </summary>
    public int MyEmptySlotCount
    {
        get
        {
            int count = 0;

            foreach (SlotScript slot in MySlots)
            {
                if (slot.IsEmpty)
                {
                    count++;
                }
            }

            return count;
        }
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Get all the items that are in this bag
    /// </summary>
    /// <returns></returns>
    public List<Item> GetItems()
    {
        List<Item> items = new List<Item>();

        foreach (SlotScript slot in slots)
        {
            if (!slot.IsEmpty)
            {
                foreach (Item item in slot.MyItems)
                {
                    items.Add(item);
                }
            }
        }

        return items;
    }

    /// <summary>
    /// Creates Slots for this bag
    /// </summary>
    /// <param name="slotCount">Amount of slots to create</param>
    public void AddSlots(int slotCount)
    {
        for (int i = 0; i < slotCount; i++)
        {
            // create a slot cannled slot and set reference to its script
            SlotScript slot = Instantiate(slotPrefab, transform).GetComponent<SlotScript>();
            // make sure the bag knows what bag it belongs to
            slot.MyBag = this;
            // add this slot to the list of slots
            MySlots.Add(slot);
        }
    }

    public bool AddItem(Item item)
    {
        foreach (SlotScript slot in MySlots)
        {
            if (slot.IsEmpty)
            {
                slot.AddItem(item);

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Opens and Closes this bag
    /// </summary>
    public void OpenClose()
    {
        canvasGroup.alpha = canvasGroup.alpha > 0 ? 0 : 1;

        canvasGroup.blocksRaycasts = canvasGroup.blocksRaycasts == true ? false : true;
    }




}
