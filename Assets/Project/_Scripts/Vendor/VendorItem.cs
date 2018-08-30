using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VendorItem
{
    [SerializeField] private Item item;

    [SerializeField] private int quantity;

    [SerializeField] private bool unlimited;

    /// <summary>
    /// Default constructor
    /// </summary>
    public VendorItem()
    {

    }

    /// <summary>
    /// Paramaterized Constructor
    /// </summary>
    /// <param name="item">Item in question</param>
    /// <param name="q">quantity of items</param>
    /// <param name="limit">unlimited or not - default is false</param>
    public VendorItem(Item item, int q, bool limit = false)
    {
        this.item = item;
        quantity = q;
        unlimited = limit;
    }

    public Item MyItem
    {
        get
        {
            return item;
        }
    }

    public int MyQuantity
    {
        get
        {
            return quantity;
        }

        set
        {
            quantity = value;
        }
    }

    public bool Unlimited
    {
        get
        {
            return unlimited;
        }
    }
}
