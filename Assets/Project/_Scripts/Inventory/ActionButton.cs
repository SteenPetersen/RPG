using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerClickHandler, IClickable {

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
        if (HandScript.instance.MyMoveable == null)
        {
            if (MyUseable != null)
            {
                MyUseable.Use();
            }
            if (useables != null && useables.Count > 0)
            {
                useables.Peek().Use();
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
            useables = InventoryScript.instance.GetUseables(useable, item);
            count = useables.Count;

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
    }

    /// <summary>
    /// This method is used when loading a game so we can update the visuals
    /// to whgatever the player had when logging off
    /// </summary>
    public void LoadGameUseable(IUseable useable, Item item)
    {
        if (useable is Item)
        {
            useables = InventoryScript.instance.GetUseables(useable, item);
            count = useables.Count;
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
        if (item is IUseable && useables.Count > 0)
        {
            string input = useables.Peek().ToString().Substring(0, useables.Peek().ToString().IndexOf("("));

            if (useables.Peek().GetType() == item.GetType() && item.name == input)
            {
                useables = InventoryScript.instance.GetUseables(item as IUseable, item);

                count = useables.Count;

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
        // if the items are stacking
        if (MyCount > 1)
        {
            MyStackText.text = MyCount.ToString();
            MyStackText.color = Color.white;
            MyIcon.color = Color.white;
        }
        // when there is only 1 item dont display text
        else
        {
            MyStackText.color = new Color(0, 0, 0, 0);
            MyIcon.color = Color.white;
        }

        // if the slot has nothing in it
        if (MyCount == 0)
        {
            MyIcon.color = new Color(0, 0, 0, 0);
            MyStackText.color = new Color(0, 0, 0, 0);
        }
    }
}
