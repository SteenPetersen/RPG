using System.Collections.Generic;
using UnityEngine;

public class Vendor : Interactable
{
    VendorWindow vendorWindow;
    [SerializeField] VendorType vendorType;

    [SerializeField] private List<VendorItem> items = new List<VendorItem>();

    public List<VendorItem> MyItems
    {
        get
        {
            return items;
        }

        set
        {
            items = value;
        }
    }
    public IntRange amountOfItems;
    bool interacting;
    Transform player; // used to measure if the player is closeby
    PlayerController pc;

    void Start()
    {
        vendorWindow = VendorWindow.instance;
        MyItems = VendorManager.instance.GetGoods(vendorType, amountOfItems);
        player = PlayerController.instance.transform;
        pc = PlayerController.instance;
    }

    void Update()
    {
        if (interacting)
        {
            if (Time.frameCount % 10 == 0)
            {
                float dist = Vector2.Distance(transform.position, player.position);

                if (dist > MyRadius * 2)
                {
                    StopInteracting();
                }
            }
        }
    }

    public override void Interact()
    {
        if (!VendorWindow.IsOpen)
        {
            interacting = true;
            VendorWindow.IsOpen = true;
            vendorWindow.MyTitle = gameObject.name;
            vendorWindow.CreatePages(MyItems);
            vendorWindow.OpenWindow();
            vendorWindow.MyVendor = this;
        }

    }

    public void StopInteracting()
    {
        if (VendorWindow.IsOpen)
        {
            interacting = false;
            VendorWindow.IsOpen = false;
            vendorWindow.CloseWindow();
            vendorWindow.MyVendor = null;
            vendorWindow.MyTitle = string.Empty;
        }
    }

    public void UpdateNewItem(VendorItem item)
    {
        foreach (VendorItem i in MyItems)
        {
            if (i.MyItem.name == item.MyItem.name)
            {
                Debug.Log("I already have " + i.MyQuantity + " items like this");
                i.MyQuantity++;
                vendorWindow.CreatePages(MyItems);
                return;
            }
        }

        MyItems.Add(item);

        vendorWindow.CreatePages(MyItems);
    }

    void OnMouseOver()
    {
        pc.mouseOverVendor = true;
    }

    void OnMouseExit()
    {
        pc.mouseOverVendor = false;
    }


}
