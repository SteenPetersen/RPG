using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerClickHandler, IClickable, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// 12.5 video
    /// </summary>
    /// 
    public IUseable MyUseable
    {
        get; set;
    }

    [SerializeField] private Text stackSize;

    private Item buttonItem;

    public string SaveItemData
    {
        get
        {
            if (buttonItem != null)
            {
                return buttonItem.name;
            }

            return string.Empty;
        }
    }

    private Stack<IUseable> useables = new Stack<IUseable>();

    private int count;

    [SerializeField]
    private Image MyIcon;

    public Button MyButton
    {
        get;
        private set;
    }

    Image IClickable.MyIcon
    {
        get
        {
            return MyIcon;
        }

        set
        {
            throw new NotImplementedException();
        }
    }

    bool displayingTooltip;

    public int MyCount
    {
        get
        {
            return count;
        }
    }

    public Text MyStackText
    {
        get { return stackSize; }
    }

    public Stack<IUseable> MyUseables
    {
        get
        {
            return useables;
        }

        set
        {
            //MyUseable = value.Peek();
            useables = value;
        }
    }

    // Use this for initialization
    void Start () {
        MyButton = GetComponent<Button>();
        MyButton.onClick.AddListener(OnClick);
        InventoryScript.instance.itemCountChangedEvent += new ItemCountChanged(UpdateItemCount);
	}
	
    /// <summary>
    /// executed when the action button is clicked
    /// </summary>
    public void OnClick()
    {
        /// Only use this if you you're not carrying another item
        if (HandScript.instance.MyMoveable == null)
        {
            if (MyUseable != null)
            {
                MyUseable.Use();
            }
            if (MyUseables != null && MyUseables.Count > 0)
            {
                MyUseables.Peek().Use();
            }
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (HandScript.instance.MyMoveable != null && HandScript.instance.MyMoveable is IUseable)
            {
                SetUseable(HandScript.instance.MyMoveable as IUseable, HandScript.instance.MyMoveable as Item);
            }
        }
    }

    public void SetUseable(IUseable useable, Item item)
    {
        if (useable is Item)
        {
            MyUseables = InventoryScript.instance.GetUseables(useable, item);
            count = MyUseables.Count;

            buttonItem = item;

            // if your trying to equip from the equipment slots
            if (InventoryScript.instance.FromSlot == null)
            {
                return;
            }
            else
            {
                // if your trying to drag a weapon, armor or ranged
                if ((int)item.typeOfEquipment <= 2)
                {
                    return;
                }
                
                InventoryScript.instance.FromSlot.MyIcon.color = Color.white;
                InventoryScript.instance.FromSlot = null;
            }

        }
        else
        {
            this.MyUseable = useable;
        }

        UpdateVisual();

        if (displayingTooltip)
        {
            UiManager.instance.RefreshToolTip(buttonItem);
        }
    }

    /// <summary>
    /// This method is used when loading a game so we can update the visuals
    /// to whgatever the player had when logging off
    /// </summary>
    public void LoadGameUseable(IUseable useable, Item item)
    {
        if (useable is Item)
        {
            MyUseables = InventoryScript.instance.GetUseables(useable, item);
            count = MyUseables.Count;
            buttonItem = item;
        }
        else
        {
            this.MyUseable = useable;
        }

        MyIcon.sprite = item.MyIcon;
        MyIcon.color = Color.white;

        if (count > 1)
        {
            UpdateStackSize();
        }
    }

    private void UpdateVisual()
    {
        MyIcon.sprite = HandScript.instance.Put().MyIcon;
        MyIcon.color = Color.white;

        UpdateStackSize();
    }

    /// <summary>
    /// Updates the count text on the UI element for the action button depending on what happens in the bags.
    /// this method is a delegate and listens to ItemCountChangedEvent on the inventory script
    /// </summary>
    /// <param name="item"></param>
    public void UpdateItemCount(Item item)
    {
        if (item is IUseable && MyUseables.Count > 0 && item.name == buttonItem.name)
        {
            if (MyUseables.Peek().GetType() == item.GetType())
            {

                MyUseables = InventoryScript.instance.GetUseables(item as IUseable, item);

                count = MyUseables.Count;


                UpdateStackSize();
            }
        }
    }

    /// <summary>
    /// Updates the text in the UI showing how many, if more than 1, 
    /// of the item we have in our bags
    /// </summary>
    public void UpdateStackSize()
    {
        UiManager.instance.UpdateStackSize(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MyUseable != null)
        {
           UiManager.instance.ShowToolTip(transform.position, buttonItem);
            displayingTooltip = true;
        }
        else if (MyUseables.Count > 0)
        {
            UiManager.instance.ShowToolTip(transform.position, buttonItem);
            displayingTooltip = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UiManager.instance.HideToolTip();
        displayingTooltip = false;
    }
}
