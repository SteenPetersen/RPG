using System;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public abstract class Item : ScriptableObject, IMoveable {

    new public string name = "New Item";
    public Sprite icon;
    public int sellValue;
    public int buyValue;
    public bool isDefaultItem = false;

    public EquipmentType typeOfEquipment;

    [SerializeField]
    private int stackSize;

    [SerializeField]
    private string title;

    [SerializeField]
    private Quality quality;

    public SlotScript MySlot;

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
    }

    public virtual void Use()
    {
        Debug.Log("Using " + name);
    }

    public virtual string GetDescription()
    {
        return string.Format("<color={0}>{1}</color>", QualityColor.MyColors[quality], title);
    }

    public void Remove()
    {
        if (MySlot != null)
        {
            MySlot.RemoveItem(this);
        }
    }
}
