using System.Collections.Generic;
using UnityEngine;

public class VendorManager : MonoBehaviour {

    public static VendorManager instance;

    public bool vendorWindowOpen;

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
    public List<Item> GetGoods(VendorType vendorType)
    {
        List<Item> items = new List<Item>();

        // potion
        if (vendorType == VendorType.Potion)
        {
           // Debug.Log("Potion Vendor is requesting goods");

            foreach (Item item in potion)
            {
                items.Add(item);
            }
        }

        // weapon
        if (vendorType == VendorType.Weapon)
        {
            //Debug.Log("Weapon Vendor is requesting goods");

            int r = UnityEngine.Random.Range(0, 6);

            for (int i = 0; i <= r; i++)
            {
                items.Add(EquipmentGenerator._instance.CreateVendorEquipment());
            }
        }

        // armor
        if (vendorType == VendorType.Armor)
        {
            //Debug.Log("Armor Vendor is requesting goods");

            foreach (Item item in armor)
            {
                items.Add(item);
            }
        }

        // general
        if (vendorType == VendorType.General)
        {
            Debug.Log("General Vendor is requesting goods");

            foreach (Item item in general)
            {
                items.Add(item);
            }
        }

        return items;
    }

    /// <summary>
    /// Sell a product
    /// </summary>
    /// <param name="value"></param>
    public void Sell(int value, Item item)
    {
        VendorWindow.instance.MyVendor.MyPurchasedStock.Add(item);
        VendorWindow.instance.MyVendor.AddItemToWindow(item);
        CurrencyManager.wealth += value;

        SoundManager.instance.PlayUiSound("lootdrop");
    }



}

public enum VendorType { Potion, Weapon, Armor, General }
