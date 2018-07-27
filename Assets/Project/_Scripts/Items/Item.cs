using System;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public abstract class Item : ScriptableObject, IMoveable, IDescribable {

    new public string name = "New Item";
    public Sprite icon;
    public int sellValue;
    public int buyValue;

    public EquipmentType typeOfEquipment;

    [SerializeField] private int stackSize;

    [SerializeField] private string title;

    [SerializeField] private Quality quality;

    public SlotScript MySlot;

    public string MyTitle
    {
        get
        {
            return title;
        }

        set
        {
            title = value;
        }

    }

    public int MyStackSize
    {
        get
        {
            return stackSize;
        }
    }

    protected SlotScript Slot
    {
        get
        {
            return MySlot;
        }

        set
        {
            MySlot = value;
        }
    }

    public Sprite MyIcon
    {
        get
        {
            return icon;
        }

        set
        {
            icon = value;
        }
    }

    public Quality MyQuality
    {
        get
        {
            return quality;
        }

        set
        {
            quality = value;
        }
    }

    public virtual void Use()
    {
        Debug.Log("Using " + name);
    }

    public virtual string GetTitle()
    {
        return string.Format("<color={0}><size=16>{1}</size></color>", QualityColor.MyColors[quality], title);
    }

    public virtual string GetDescription(bool showSaleValue = true)
    {
        //return string.Format("<color={0}><size=16>{1}</size></color>", QualityColor.MyColors[quality], title);
        return null;
    }

    public void Remove()
    {
        if (MySlot != null)
        {
            MySlot.RemoveItem(this);
        }
    }
}
