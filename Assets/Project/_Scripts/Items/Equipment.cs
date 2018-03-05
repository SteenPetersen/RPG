using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item {

    public EquipmentSlot equipSlot;
    public EquipmentType equipType;
    public Sprite characterVisibleSprite;

    public int armorModifier;
    public int damageModifier;

    public int rangedProjectile;

    public override void Use()
    {
        base.Use();
        // equip the items
        //Debug.Log("Attempting to equip");
        bool equipIt = EquipmentManager.instance.Equip(this);
        //remove it from inventory if it managed to get equipped
        if (equipIt)
        {
            RemoveFromInventory();
        }
    }


}

public enum EquipmentSlot { Head, Chest, Legs, MainHand, OffHand, FrontFoot, BackFoot, GauntletLeft, GauntletRight, Shoulder, Ring1, Ring2, Neck }
public enum EquipmentType { Melee, Ranged, Armor, Key, Light, Potion}
