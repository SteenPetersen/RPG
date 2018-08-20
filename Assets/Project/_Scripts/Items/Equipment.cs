using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item, IUseable {

    public EquipmentSlot equipSlot;
    public EquipmentType equipType;
    public ArmorType armorType;
    public Sprite characterVisibleSprite;

    [SerializeField] Sprite glowEffect;

    public int tier;

    public int armorModifier;
    public int damageModifier;
    public int sta;
    public int agi;
    public int str;

    public int rangedProjectile;

    public Sprite MyGlowSprite
    {
        get
        {
            return glowEffect;
        }

        set
        {
            glowEffect = value;
        }
    }

    public new Sprite MyIcon
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

    public int graphicId;

    public override void Use()
    {
        if ((int)equipType != 5)
        {
            bool equipIt = EquipmentManager.instance.Equip(this);
            //remove it from inventory if it managed to get equipped
            if (equipIt)
            {
                Remove();
            }
        }
    }


    public override string GetDescription(bool showSaleValue = true)
    {
        string info = base.GetDescription() + string.Format("Damage: {0}\nArmor: {1}", damageModifier, armorModifier);

        if (sta > 0)
        {
            info = info + "\nSta:" + sta;
        }
        if (str > 0)
        {
            info = info + "\nStr:" + str;
        }
        if (agi > 0)
        {
            info = info + "\nAgi:" + agi;
        }

        if (VendorManager.instance.vendorWindowOpen && showSaleValue)
        {
            info = info + "\n\nSell:" + sellValue;
        }

        return info;
    }


}

public enum EquipmentSlot { Head, Chest, Legs, MainHand, OffHand, FrontFoot, BackFoot, GauntletLeft, GauntletRight, Shoulder, Ring1, Ring2, Neck }
public enum EquipmentType { Melee, Ranged, Armor, Key, Light, Potion, SpellBook}
public enum ArmorType { Leather, Metal, Magical}
