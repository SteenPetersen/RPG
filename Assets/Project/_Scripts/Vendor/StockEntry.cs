using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StockEntry : MonoBehaviour, IPointerClickHandler {

    [SerializeField]
    Image icon;
    [SerializeField]
    Text itemName;
    [SerializeField]
    Text sellPrice;
    [SerializeField]
    Item item;

    public bool isDefaultStock;

    public Sprite StockIcon
    {
        get
        {
            return icon.sprite;
        }

        set
        {
            if (value != null)
            {
                icon.color = Color.white;
            }
            else if (value == null)
            {
                icon.color = new Color(0, 0, 0, 0);
            }
            icon.sprite = value;
        }
    }

    public Item MyItem
    {
        get
        {
            return item;
        }

        set
        {
            item = value;
        }
    }

    public string StockItemName
    {
        get
        {
            return itemName.text;
        }
        set
        {
            itemName.text = value;
        }
    }

    public string StockItemPrice
    {
        get
        {
            return sellPrice.text;
        }
        set
        {
            sellPrice.text = value;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log(name + " Game Object Right Clicked!");

            if (CurrencyManager.wealth >= MyItem.buyValue && InventoryScript.instance.AddItem(item))
            {
                CurrencyManager.wealth -= MyItem.buyValue;
                SoundManager.instance.PlayUiSound("purchase");
            }

            if (!isDefaultStock)
            {
                Destroy(gameObject);
            }
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log(name + " Game Object Left Clicked!");
        }
    }
}
