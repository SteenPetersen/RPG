using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentRepo : MonoBehaviour {

    public static EquipmentRepo _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
        }
    }

    public List<ItemGraphicSet> swords = new List<ItemGraphicSet>();

    public List<ItemGraphicSet> bows = new List<ItemGraphicSet>();

    public List<ArmorGraphicSet> shields = new List<ArmorGraphicSet>();

    public List<ArmorGraphicSet> leatherArmorHelm = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> leatherArmorChest = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> leatherArmorLegs = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> leatherArmorFrontfoot = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> leatherArmorBackfoot = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> leatherArmorGuantleft = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> leatherArmorGuantright = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> leatherArmorShoulder = new List<ArmorGraphicSet>();

    public List<ArmorGraphicSet> metalArmorHelm = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> metalArmorChest = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> metalArmorLegs = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> metalArmorFrontfoot = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> metalArmorBackfoot = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> metalArmorGuantleft = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> metalArmorGuantright = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> metalArmorShoulder = new List<ArmorGraphicSet>();

    public List<ArmorGraphicSet> magicalArmorHelm = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> magicalArmorChest = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> magicalArmorLegs = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> magicalArmorFrontfoot = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> magicalArmorBackfoot = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> magicalArmorGuantleft = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> magicalArmorGuantright = new List<ArmorGraphicSet>();
    public List<ArmorGraphicSet> magicalArmorShoulder = new List<ArmorGraphicSet>();

    public ItemGraphicSet GetMeleeItemSet(int identification)
    {
        foreach (ItemGraphicSet item in swords)
        {
            if (item.id == identification)
            {
                return item;
            }
        }

        Debug.LogWarning("didnt find the corresponding melee set");
        return null;
    }

    public ArmorGraphicSet GetShieldSet(int identification)
    {
        foreach (ArmorGraphicSet item in shields)
        {
            if (item.id == identification)
            {
                return item;
            }
        }

        Debug.LogWarning("didnt find the corresponding melee set");
        return null;
    }

    public ItemGraphicSet GetRangedItemSet(int identification)
    {
        foreach (ItemGraphicSet item in bows)
        {
            if (item.id == identification)
            {
                return item;
            }
        }

        Debug.LogWarning("didnt find the corresponding ranged set");
        return null;
    }

    public ArmorGraphicSet GetArmorSet(int identification, ArmorType armorType, EquipmentSlot slot)
    {
        if (armorType == ArmorType.Leather)
        {

        }

        else if (armorType == ArmorType.Metal)
        {
            switch (slot)
            {
                case EquipmentSlot.Head:

                    foreach (ArmorGraphicSet armor in metalArmorHelm)
                    {
                        if (armor.id == identification)
                        {
                            return armor;
                        }
                    }

                    Debug.LogWarning("didnt find the corresponding ranged set");
                    break;

                case EquipmentSlot.Chest:

                    foreach (ArmorGraphicSet armor in metalArmorChest)
                    {
                        if (armor.id == identification)
                        {
                            return armor;
                        }
                    }

                    Debug.LogWarning("didnt find the corresponding ranged set");
                    break;

                case EquipmentSlot.Legs:

                    foreach (ArmorGraphicSet armor in metalArmorLegs)
                    {
                        if (armor.id == identification)
                        {
                            return armor;
                        }
                    }
                    break;

                case EquipmentSlot.FrontFoot:

                    foreach (ArmorGraphicSet armor in metalArmorFrontfoot)
                    {
                        if (armor.id == identification)
                        {
                            return armor;
                        }
                    }
                    break;

                case EquipmentSlot.BackFoot:

                    foreach (ArmorGraphicSet armor in metalArmorBackfoot)
                    {
                        if (armor.id == identification)
                        {
                            return armor;
                        }
                    }
                    break;

                case EquipmentSlot.GauntletLeft:

                    foreach (ArmorGraphicSet armor in metalArmorGuantleft)
                    {
                        if (armor.id == identification)
                        {
                            return armor;
                        }
                    }
                    break;

                case EquipmentSlot.GauntletRight:

                    foreach (ArmorGraphicSet armor in metalArmorGuantright)
                    {
                        if (armor.id == identification)
                        {
                            return armor;
                        }
                    }
                    break;

                case EquipmentSlot.Shoulder:

                    foreach (ArmorGraphicSet armor in metalArmorShoulder)
                    {
                        if (armor.id == identification)
                        {
                            return armor;
                        }
                    }
                    break;
            }
        }

        else if (armorType == ArmorType.Magical)
        {

        }

        return null;
    }


}

[System.Serializable]
public class ItemGraphicSet
{
    public int id;
    public Sprite visible;
    public Sprite glow;
    public Sprite icon;
}

[System.Serializable]
public class ArmorGraphicSet
{
    public int id;
    public Sprite visible;
    public Sprite icon;
}
