using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Bag", menuName ="Items/Bag", order =1)]
public class Bag : Item {

    [SerializeField] private int slots;

    [SerializeField]
    private GameObject bagPrefab;

    /// <summary>
    /// a reference to the bagScript that this bag belongs to
    /// </summary>
    public BagScript MyBagScript { get; set; }

    /// <summary>
    /// a reference to the bagbutton that belongs to this bag
    /// </summary>
    public BagButton MyBagButton { get; set; }

    public int Slots
    {
        get
        {
            return slots;
        }
    }

    public void Initialize(int slots)
    {
        this.slots = slots;
    }

    /// <summary>
    /// Equips a bag
    /// </summary>
    public override void Use()
    {
        if (InventoryScript.instance.CanAddBag)
        {
            Remove();
            MyBagScript = Instantiate(bagPrefab, InventoryScript.instance.transform).GetComponent<BagScript>();
            MyBagScript.AddSlots(slots);

            if (MyBagButton == null)
            {
                InventoryScript.instance.AddBag(this);
            }
            else
            {
                InventoryScript.instance.AddBag(this, MyBagButton);
            }

        }
    }

    public override string GetDescription()
    {
        return base.GetDescription() + string.Format("\n{0} Slot Bag", slots);
    }
}
