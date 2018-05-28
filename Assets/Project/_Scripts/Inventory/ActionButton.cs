using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerClickHandler, IClickable {

    public IUseable MyUseable { get; set; }

    [SerializeField]
    private Text stackSize;

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
	
	// Update is called once per frame
	void Update () {
		
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

    private void UpdateVisual()
    {
        MyIcon.sprite = HandScript.instance.Put().MyIcon;
        MyIcon.color = Color.white;

        if (count > 1)
        {
            UiManager.instance.UpdateStackSize(this);
        }
    }

    public void UpdateItemCount(Item item)
    {
        if (item is IUseable && useables.Count > 0)
        {
            if (useables.Peek().GetType() == item.GetType())
            {
                useables = InventoryScript.instance.GetUseables(item as IUseable, item);

                count = useables.Count;

                UiManager.instance.UpdateStackSize(this);
            }
        }
    }
}
