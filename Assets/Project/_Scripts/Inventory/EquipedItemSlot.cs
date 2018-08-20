using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipedItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    EquipmentManager equipManager;

    public Image MyIcon;
    public int slotId;

    private Equipment equipment;

    public Equipment MyEquipment
    {
        get
        {
            return equipment;
        }

        set
        {
            equipment = value;
        }
    }


    private void Start()
    {
        equipManager = EquipmentManager.instance;
    }

    /// <summary>
    /// Adds the item to the a visible slot
    /// </summary>
    /// <param name="newItem">Item to be added</param>
    /// <param name="silent">Bool used in case we are loading a gaem and dont wish to have sound effects</param>
    public void AddItem(Item newItem, bool silent = false)
    {
        equipment = (Equipment)newItem;

        if (!silent)
        {
            SoundManager.instance.PlayInventorySound("AddItem");
        }

        MyIcon.sprite = MyEquipment.icon;
        MyIcon.enabled = true;
    }

    public void ClearSLot()
    {
        UnequipFromInventory();
        MyEquipment = null;
        MyIcon.sprite = null;
        MyIcon.enabled = false;
    }

    public void DragClearSlot()
    {
        DragUnequip();
        MyEquipment = null;
        MyIcon.sprite = null;
        MyIcon.enabled = false;
    }

    public void UseItem()
    {
        if (MyEquipment != null)
        {
            MyEquipment.Use();
        }
    }

    public void DragUnequip()
    {
        equipManager.UnequipByDragging(slotId);
    }

    public void UnequipFromInventory()
    {
        equipManager.Unequip(slotId);
    }

    /// <summary>
    /// Determines what happens when clicking on the equipment slot
    /// </summary>
    public void OnCLick()
    {
        if (InventoryScript.instance.FromSlot == null)
        {
            // if I have nothing in my hand
            if (HandScript.instance.MyMoveable == null)
            {
                HandScript.instance.TakeMoveable(equipment as IMoveable);
                InventoryScript.instance.FromEqippedSlot = this;
            }
        }

        else if (InventoryScript.instance.FromSlot != null)
        {
            bool matchingSlot = equipManager.CheckIfSlotMatchesType(slotId, HandScript.instance.MyMoveable as Equipment);

            if (matchingSlot)
            {
                bool equipSuccess = equipManager.Equip(HandScript.instance.MyMoveable as Equipment);

                if (equipSuccess)
                {
                    HandScript.instance.Drop();
                    InventoryScript.instance.FromSlot.Clear();
                    InventoryScript.instance.FromSlot = null;
                }
            }
        }
    }

    private void OnDisable()
    {
        //Debug.Log("Called OnDisabled on EquipedItemSlot");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // show tooltip
        if (MyEquipment != null)
        {
            UiManager.instance.ShowToolTipEquipmentView(MyEquipment);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // hide tooltip
        UiManager.instance.HideToolTip();
    }
}
