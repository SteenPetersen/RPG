using UnityEngine;
using UnityEngine.UI;

public class EquipedItemSlot : MonoBehaviour
{
    EquipmentManager equipManager;

    public Image icon;
    public Button removeButton;
    public int slotId;

    Item item;

    private void Start()
    {
        equipManager = EquipmentManager.instance;
    }

    public void AddItem(Item newItem)
    {
        item = newItem;

        SoundManager.instance.PlayInventorySound("AddItem");

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

    public void UseItem()
    {
        if (item != null)
        {
            item.Use();
        }
    }

    public void UnequipFromInventory()
    {
        equipManager.Unequip(slotId);
    }

    private void OnDisable()
    {
        Debug.Log("Called OnDisabled on EquipedItemSlot");
    }
}
