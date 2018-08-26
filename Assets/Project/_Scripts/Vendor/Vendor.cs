using System.Collections.Generic;
using UnityEngine;

public class Vendor : MonoBehaviour
{
    PlayerController player;

    [SerializeField] VendorType vendorType;

    public float radius;
    GameObject vendorUi;
    CanvasGroup vendorWindow;

    [SerializeField] Transform speechLocation;

    [SerializeField] StockEntry stockEntry;

    [SerializeField] List<Item> defaultStock;

    [SerializeField] List<Item> purchasedStock;

    public List<Item> MyDefaultStock
    {
        get
        {
            return defaultStock;
        }
    }

    public List<Item> MyPurchasedStock
    {
        get
        {
            return purchasedStock;
        }
    }

    public Transform MySpeechLocation
    {
        get
        {
            return speechLocation;
        }
    }

    void Start()
    {
        player = PlayerController.instance;
        vendorUi = GameObject.Find("VendorUi");
        vendorWindow = vendorUi.GetComponentInParent<CanvasGroup>();
        defaultStock = VendorManager.instance.GetGoods(vendorType);
    }

    private void Update()
    {
        if (VendorManager.instance.vendorWindowOpen)
        {
            float distance = Vector2.Distance(PlayerController.instance.transform.position, 
                                              VendorWindow.instance.MyVendor.transform.position);

            // if the player moves away from the vendor then close the window
            if (distance > radius * 4)
            {
                OpenClose();
            }
        }
    }

    /// <summary>
    /// Display the vendor window and show a stock entry for each item in stock
    /// </summary>
    public void Interact()
    {
        if (!VendorManager.instance.vendorWindowOpen)
        {
            int count = 1;

            // must delete everything everytime because we use the same window for all vendors.
            foreach (Transform child in vendorUi.transform)
            {
                Destroy(child.gameObject);
            }

            VendorWindow.instance.MyVendor = this;
            VendorWindow.instance.MyTitle = gameObject.name;

            // run through each item in stocvk and place them in the vendorUI window
            foreach (Item item in defaultStock)
            {
                GameObject entry = Instantiate(stockEntry.gameObject, vendorUi.transform);
                StockEntry stockData = entry.GetComponent<StockEntry>();

                stockData.StockIcon = item.icon;
                stockData.MyItem = item;
                stockData.isDefaultStock = true;
                stockData.StockItemName = item.name;
                stockData.StockItemPrice = item.buyValue.ToString();

                count++;
            }

            foreach (Item item in purchasedStock)
            {
                if (count <= 6)
                {
                    GameObject entry = Instantiate(stockEntry.gameObject, vendorUi.transform);
                    StockEntry stockData = entry.GetComponent<StockEntry>();

                    stockData.StockIcon = item.icon;
                    stockData.MyItem = item;
                    stockData.isDefaultStock = false;
                    stockData.StockItemName = item.name;
                    stockData.StockItemPrice = item.buyValue.ToString();
                }

                count++;
            }

            OpenClose();

        }
    }

    /// <summary>
    /// Opens and closes the vendor window
    /// </summary>
    public void OpenClose()
    {
        // if the vendor UI menu's alpha is above zero set it to zero, else set it to 1
        vendorWindow.alpha = vendorWindow.alpha > 0 ? 0 : 1;

        // make sure you can block raycasts
        vendorWindow.blocksRaycasts = vendorWindow.blocksRaycasts == true ? false : true;

        // make sure all scripts can know if the window is open or not
        VendorManager.instance.vendorWindowOpen = VendorManager.instance.vendorWindowOpen == true ? false : true;
    }

    /// <summary>
    /// Add an item to the window, called when player sells an item to the vendor
    /// </summary>
    /// <param name="item"></param>
    public void AddItemToWindow(Item item)
    {
        if (purchasedStock.Count + defaultStock.Count <= 6)
        {
            GameObject entry = Instantiate(stockEntry.gameObject, vendorUi.transform);
            StockEntry stockData = entry.GetComponent<StockEntry>();

            stockData.StockIcon = item.icon;
            stockData.MyItem = item;
            stockData.isDefaultStock = false;
            stockData.StockItemName = item.name;
            stockData.StockItemPrice = item.sellValue.ToString();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    /// <summary>
    /// Keeps tabs on when mouse is over the vendor 
    /// to stop player from shooting etc
    /// </summary>
    void OnMouseOver()
    {
        if (!player.mouseOverVendor)
        {
            player.mouseOverVendor = true;
        }
    }

    /// <summary>
    /// keepos tabs on when mouse leaves vendor
    /// to reallow player to shoot and hit etc
    /// </summary>
    void OnMouseExit()
    {
        if (player.mouseOverVendor)
        {
            player.mouseOverVendor = false;
        }
    }
}
