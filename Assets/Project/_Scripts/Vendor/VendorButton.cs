using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class VendorButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] Image icon;

    [SerializeField] TextMeshProUGUI title;

    [SerializeField] TextMeshProUGUI sellPrice;

    [SerializeField] TextMeshProUGUI quantity;

    [SerializeField] VendorItem vendorItem;

    public void AddItem(VendorItem vendorItem)
    {
        this.vendorItem = vendorItem;

        if (vendorItem.MyQuantity > 0 || (vendorItem.MyQuantity == 0 && vendorItem.Unlimited))
        {
            icon.sprite = vendorItem.MyItem.MyIcon;
            title.text = string.Format("<color={0}>{1}</color>", QualityColor.MyColors[vendorItem.MyItem.MyQuality], vendorItem.MyItem.MyTitle);
            

            if (!vendorItem.Unlimited)
            {
                quantity.text = vendorItem.MyQuantity.ToString();
            }
            else
            {
                quantity.text = string.Empty;
            }

            if (vendorItem.MyItem.buyValue > 0)
            {
                sellPrice.text = vendorItem.MyItem.buyValue.ToString();
            }

            else
            {
                sellPrice.text = string.Empty;
            }

            gameObject.SetActive(true);
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CurrencyManager.wealth >= vendorItem.MyItem.buyValue && InventoryScript.instance.AddItem(vendorItem.MyItem))
        {
            SellItem();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UiManager.instance.ShowToolTip(transform.position, vendorItem.MyItem, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UiManager.instance.HideToolTip();
    }

    void SellItem()
    {
        CurrencyManager.wealth -= vendorItem.MyItem.buyValue;

        if (!vendorItem.Unlimited)
        {
            vendorItem.MyQuantity--;
            quantity.text = vendorItem.MyQuantity.ToString();

            if (vendorItem.MyQuantity == 0)
            {
                gameObject.SetActive(false);
                UiManager.instance.HideToolTip();
            }
        }
    }
}
