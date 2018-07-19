using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item, IUseable {

    public EquipmentSlot equipSlot;
    public EquipmentType equipType;
    public Sprite characterVisibleSprite;

    [SerializeField] Sprite glowEffect;

    public int armorModifier;
    public int damageModifier;

    public int rangedProjectile;

    public Sprite MyGlowSprite
    {
        get
        {
            return glowEffect;
        }
    }

    public new Sprite MyIcon
    {
        get
        {
            return icon;
        }
    }

    public override void Use()
    {
        Debug.Log("trying to use equipment");
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

    public override string GetDescription()
    {
        return base.GetDescription() + string.Format("\nDamage: {0}\nArmor: {1}", damageModifier, armorModifier);
    }


}

public enum EquipmentSlot { Head, Chest, Legs, MainHand, OffHand, FrontFoot, BackFoot, GauntletLeft, GauntletRight, Shoulder, Ring1, Ring2, Neck }
public enum EquipmentType { Melee, Ranged, Armor, Key, Light, Potion}
