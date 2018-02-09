using UnityEngine;
using UnityEngine.UI;

public class EquipedItemSlot : MonoBehaviour
{

    public Image icon;
    public Button removeButton;
    public int slotId;

    Item item;

    public void AddItem(Item newItem)
    {
        item = newItem;

        icon.sprite = item.icon;
        icon.enabled = true;
        removeButton.interactable = true;
    }

    public void ClearSLot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
        removeButton.interactable = false;
    }

    //public void OnRemoveButton()
    //{
    //    Inventory.instance.Remove(item);
    //}

    //public void UseItem()
    //{
    //    if (item != null)
    //    {
    //        item.Use();
    //    }
    //}
}
