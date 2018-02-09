using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item {

    public EquipmentSlot equipSlot;
    public Sprite characterVisibleSprite;

    public int armorModifier;
    public int damageModifier;

    public override void Use()
    {
        base.Use();
        // equip the items
        EquipmentManager.instance.Equip(this);
        //remove it from inventory
        RemoveFromInventory();
    }


}

public enum EquipmentSlot { Head, Chest, Legs, MainHand, OffHand, FrontFoot, BackFoot, Ring1, Ring2, Neck }
