using System.Collections.Generic;
using UnityEngine;

public class VendorManager : MonoBehaviour {

    public static VendorManager instance;

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

    }

    [SerializeField]
    private List<Item> potion = new List<Item>();
    [SerializeField]
    private List<Item> armor = new List<Item>();
    [SerializeField]
    private List<Item> general = new List<Item>();


    void Start () {




    }

    /// <summary>
    /// Method call made by vendors in order to populate their stock
    /// </summary>
    /// <param name="vendorType">Type of vendor that is looking for items</param>
    /// <returns>a list of items related to the vendor</returns>
    public List<VendorItem> GetGoods(VendorType vendorType, IntRange amount)
    {
        List<VendorItem> items = new List<VendorItem>();

        // potion
        if (vendorType == VendorType.Potion)
        {
            foreach (Item item in potion)
            {
                int rand = amount.Random;
                VendorItem tmp = new VendorItem(item, rand);
                items.Add(tmp);
            }
        }

        // weapon
        if (vendorType == VendorType.Weapon)
        {
            int r = UnityEngine.Random.Range(0, 14);

            for (int i = 0; i <= r; i++)
            {
                VendorItem tmp = new VendorItem(EquipmentGenerator._instance.CreateVendorEquipment(0), 1);
                items.Add(tmp);
            }
        }

        //// armor
        //if (vendorType == VendorType.Armor)
        //{
        //    //Debug.Log("Armor Vendor is requesting goods");

        //    foreach (Item item in armor)
        //    {
        //        items.Add(item);
        //    }
        //}

        //// general
        //if (vendorType == VendorType.General)
        //{
        //    Debug.Log("General Vendor is requesting goods");

        //    foreach (Item item in general)
        //    {
        //        items.Add(item);
        //    }
        //}

        return items;
    }

    /// <summary>
    /// Sell a product
    /// </summary>
    /// <param name="value"></param>
    public void Sell(int value, Item item)
    {
        //VendorWindow.instance.MyVendor.MyStock.Add(item);
        //VendorWindow.instance.MyVendor.AddItemToWindow(item);
        VendorItem newVendorItem = new VendorItem(item, 1);
            
        VendorWindow.instance.MyVendor.UpdateNewItem(newVendorItem);

        CurrencyManager.wealth += value;

        SoundManager.instance.PlayUiSound("lootdrop");
    }



}

public enum VendorType { Potion, Weapon, Armor, General }
