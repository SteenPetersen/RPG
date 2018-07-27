using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentGenerator : MonoBehaviour {

    public static EquipmentGenerator _instance;

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

    public GameObject droppableGameObject;

    /// <summary>
    /// Creates an Item : for use when determining vendor loot
    /// </summary>
    /// <param name="pos"></param>
    public Item CreateVendorEquipment()
    {
        Item equip = CreateEquipment() as Item;

        return equip as Item;
    }

    /// <summary>
    /// Creates a droppable GameObject for use when determining enemy and Chest loot
    /// </summary>
    /// <param name="pos"></param>
    public GameObject CreateDroppable()
    {
        GameObject tmp = Instantiate(droppableGameObject);

        Equipment equip = CreateEquipment();

        tmp.GetComponent<ItemPickup>().item = equip;
        tmp.GetComponent<SpriteRenderer>().sprite = equip.icon;
        tmp.name = equip.name;

        return tmp;
    }

    /// <summary>
    /// Creates a droppable Item into the world
    /// </summary>
    /// <param name="pos"></param>
    public void CreateDroppable(Vector3 pos)
    {
        GameObject tmp = Instantiate(droppableGameObject, pos, Quaternion.identity);

        Equipment equip = CreateEquipment();

        tmp.GetComponent<ItemPickup>().item = equip;
        tmp.GetComponent<SpriteRenderer>().sprite = equip.icon;
        tmp.name = equip.name;
    }

    /// <summary>
    /// Create a predetermined item, used for loading items that have been saved by the player
    /// </summary>
    /// <param name="item">Item to load data from</param>
    public Equipment CreateLoadedItem(ItemData item)
    {
        Equipment equip = ScriptableObject.CreateInstance<Equipment>();

        equip.name = item.name;
        equip.MyTitle = item.title;
        equip.graphicId = item.graphicId;
        equip.armorType = item.armorType;

        equip.equipType = item.type;
        equip.equipSlot = item.slot;
        equip.MyQuality = item.quality;

        if (equip.equipType == EquipmentType.Melee)
        {
            ItemGraphicSet tmp = EquipmentRepo._instance.GetMeleeItemSet(item.graphicId);
            equip.characterVisibleSprite = tmp.visible;
            equip.MyGlowSprite = tmp.glow;
            equip.MyIcon = tmp.icon;
        }
        else if (equip.equipType == EquipmentType.Ranged)
        {
            ItemGraphicSet tmp = EquipmentRepo._instance.GetRangedItemSet(item.graphicId);
            equip.characterVisibleSprite = tmp.visible;
            equip.MyGlowSprite = tmp.glow;
            equip.MyIcon = tmp.icon;
        }
        else if (equip.equipType == EquipmentType.Armor)
        {
            if (equip.equipSlot == EquipmentSlot.OffHand)
            {
                ArmorGraphicSet shield = EquipmentRepo._instance.GetShieldSet(item.graphicId);
                equip.characterVisibleSprite = shield.visible;
                equip.MyIcon = shield.icon;
            }
            else
            {
                //Debug.Log("Generating armor " + item.graphicId + " " + item.armorType + " " + item.slot + " " + item.name);

                ArmorGraphicSet tmp = EquipmentRepo._instance.GetArmorSet(item.graphicId, item.armorType, item.slot);
                equip.characterVisibleSprite = tmp.visible;
                equip.MyIcon = tmp.icon;
            }

        }

        equip.damageModifier = item.dmgMod;
        equip.armorModifier = item.armorMod;
        equip.sta = item.sta;
        equip.str = item.str;
        equip.agi = item.agi;
        equip.rangedProjectile = item.rangedProjectile;
        equip.sellValue = item.sellValue;
        equip.buyValue = item.buyValue;

        return equip;
    }

    /// Handles creating a randomized Item based on player level
    /// </summary>
    /// <returns></returns>
    public Equipment CreateEquipment()
    {
        Equipment currentCreation = ScriptableObject.CreateInstance<Equipment>();

        int n = UnityEngine.Random.Range(0, 3);

        Debug.Log(n);

        int playerLvl = ExperienceManager.instance.level;

        // melee weapon
        if (n == 0)
        {
            CreateSword(currentCreation, playerLvl);
        }
        // bow
        else if (n == 1)
        {
            CreateBow(currentCreation, playerLvl);
        }
        // armor
        else if (n == 2)
        {
            int rand = UnityEngine.Random.Range(0, 100);

            if (rand >= 90)
            {
                CreateShield(currentCreation, playerLvl);
            }
            else
            {
                CreateArmor(currentCreation, playerLvl);
            }
        }

        return currentCreation;
    }

    /// <summary>
    /// Creates a bow for the currentCreation
    /// </summary>
    /// <param name="currentCreation">Current item under creation</param>
    /// <param name="playerLvl">Current Player level</param>
    private void CreateBow(Equipment currentCreation, int playerLvl)
    {
        currentCreation.equipType = EquipmentType.Ranged;
        currentCreation.typeOfEquipment = EquipmentType.Ranged;
        currentCreation.equipSlot = EquipmentSlot.MainHand;

        currentCreation.MyQuality = DetermineRarity();

        if ((int)currentCreation.MyQuality == 0)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.bows.Count);

            currentCreation.graphicId = EquipmentRepo._instance.bows[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.bows[graphic].visible;
            currentCreation.MyGlowSprite = EquipmentRepo._instance.bows[graphic].glow;
            currentCreation.MyIcon = EquipmentRepo._instance.bows[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(1, playerLvl + 3);
            currentCreation.armorModifier = UnityEngine.Random.Range(0, (playerLvl / 10));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 10), (playerLvl + 10 * 2));
        }

        else if ((int)currentCreation.MyQuality == 1)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.bows.Count);

            currentCreation.graphicId = EquipmentRepo._instance.bows[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.bows[graphic].visible;
            currentCreation.MyGlowSprite = EquipmentRepo._instance.bows[graphic].glow;
            currentCreation.MyIcon = EquipmentRepo._instance.bows[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(playerLvl + 3, playerLvl + 5);
            currentCreation.armorModifier = UnityEngine.Random.Range(0, (playerLvl / 6));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 15), (playerLvl + 15 * 2));
        }

        else if ((int)currentCreation.MyQuality == 2)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.bows.Count);

            currentCreation.graphicId = EquipmentRepo._instance.bows[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.bows[graphic].visible;
            currentCreation.MyGlowSprite = EquipmentRepo._instance.bows[graphic].glow;
            currentCreation.MyIcon = EquipmentRepo._instance.bows[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(playerLvl + 5, playerLvl + 10);
            currentCreation.armorModifier = UnityEngine.Random.Range(0, (playerLvl / 2));

            currentCreation.sta = UnityEngine.Random.Range(0, (playerLvl / 4));
            currentCreation.str = UnityEngine.Random.Range(0, (playerLvl / 4));
            currentCreation.agi = UnityEngine.Random.Range(0, (playerLvl / 4));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 35), (playerLvl + 35 * 2));
        }

        else if ((int)currentCreation.MyQuality == 3)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.bows.Count);

            currentCreation.graphicId = EquipmentRepo._instance.bows[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.bows[graphic].visible;
            currentCreation.MyGlowSprite = EquipmentRepo._instance.bows[graphic].glow;
            currentCreation.MyIcon = EquipmentRepo._instance.bows[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(playerLvl + 10, playerLvl + 15);
            currentCreation.armorModifier = UnityEngine.Random.Range(0, (playerLvl));

            currentCreation.sta = UnityEngine.Random.Range(2, (playerLvl / 2));
            currentCreation.str = UnityEngine.Random.Range(2, (playerLvl / 2));
            currentCreation.agi = UnityEngine.Random.Range(2, (playerLvl / 2));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 113), (playerLvl + 113 * 2));
        }

        else if ((int)currentCreation.MyQuality == 4)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.bows.Count);

            currentCreation.graphicId = EquipmentRepo._instance.bows[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.bows[graphic].visible;
            currentCreation.MyGlowSprite = EquipmentRepo._instance.bows[graphic].glow;
            currentCreation.MyIcon = EquipmentRepo._instance.bows[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(playerLvl + 15, playerLvl + 25);
            currentCreation.armorModifier = UnityEngine.Random.Range(0, (playerLvl * 2));

            currentCreation.sta = UnityEngine.Random.Range((playerLvl / 2), (playerLvl));
            currentCreation.str = UnityEngine.Random.Range((playerLvl / 2), (playerLvl));
            currentCreation.agi = UnityEngine.Random.Range((playerLvl / 2), (playerLvl));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 350), (playerLvl + 350 * 4));
        }

        currentCreation.buyValue = currentCreation.sellValue * 6;

        string currentName = EquipNameGenerator._instance.GetBowName(currentCreation.MyQuality);
        currentCreation.MyTitle = currentName;
        currentCreation.name = currentName;

        currentCreation.rangedProjectile = 0;
    }

    /// <summary>
    /// Creates armor for the currentCreation
    /// </summary>
    /// <param name="currentCreation">Current item under creation</param>
    /// <param name="playerLvl">Current Player level</param>
    private void CreateArmor(Equipment currentCreation, int playerLvl)
    {
        currentCreation.equipType = EquipmentType.Armor;
        currentCreation.typeOfEquipment = EquipmentType.Armor;
        currentCreation.armorType = ArmorType.Metal;

        currentCreation.equipSlot = DetermineArmorSlot();
        currentCreation.MyQuality = DetermineRarity();

        int material = UnityEngine.Random.Range(1, 1); // TODO set it so it can make leather and magical later

        // leather
        if (currentCreation.armorType == ArmorType.Leather)
        {

        }
        // metal
        else if (currentCreation.armorType == ArmorType.Metal)
        {
            // its a helm
            if ((int)currentCreation.equipSlot == 0)
            {
                int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.metalArmorHelm.Count);

                currentCreation.graphicId = EquipmentRepo._instance.metalArmorHelm[graphic].id;

                currentCreation.characterVisibleSprite = EquipmentRepo._instance.metalArmorHelm[graphic].visible;
                currentCreation.MyIcon = EquipmentRepo._instance.metalArmorHelm[graphic].icon;
            }
            // its a chest
            if ((int)currentCreation.equipSlot == 1)
            {
                int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.metalArmorChest.Count);

                currentCreation.graphicId = EquipmentRepo._instance.metalArmorChest[graphic].id;

                currentCreation.characterVisibleSprite = EquipmentRepo._instance.metalArmorChest[graphic].visible;
                currentCreation.MyIcon = EquipmentRepo._instance.metalArmorChest[graphic].icon;
            }
            // its legs
            if ((int)currentCreation.equipSlot == 2)
            {
                int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.metalArmorLegs.Count);

                currentCreation.graphicId = EquipmentRepo._instance.metalArmorLegs[graphic].id;

                currentCreation.characterVisibleSprite = EquipmentRepo._instance.metalArmorLegs[graphic].visible;
                currentCreation.MyIcon = EquipmentRepo._instance.metalArmorLegs[graphic].icon;
            }
            // its front foot
            if ((int)currentCreation.equipSlot == 5)
            {
                int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.metalArmorFrontfoot.Count);

                currentCreation.graphicId = EquipmentRepo._instance.metalArmorFrontfoot[graphic].id;

                currentCreation.characterVisibleSprite = EquipmentRepo._instance.metalArmorFrontfoot[graphic].visible;
                currentCreation.MyIcon = EquipmentRepo._instance.metalArmorFrontfoot[graphic].icon;
            }
            // its back foot
            if ((int)currentCreation.equipSlot == 6)
            {
                int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.metalArmorBackfoot.Count);

                currentCreation.graphicId = EquipmentRepo._instance.metalArmorBackfoot[graphic].id;

                currentCreation.characterVisibleSprite = EquipmentRepo._instance.metalArmorBackfoot[graphic].visible;
                currentCreation.MyIcon = EquipmentRepo._instance.metalArmorBackfoot[graphic].icon;
            }
            // left gaunt
            if ((int)currentCreation.equipSlot == 7)
            {
                int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.metalArmorGuantleft.Count);

                currentCreation.graphicId = EquipmentRepo._instance.metalArmorGuantleft[graphic].id;

                currentCreation.characterVisibleSprite = EquipmentRepo._instance.metalArmorGuantleft[graphic].visible;
                currentCreation.MyIcon = EquipmentRepo._instance.metalArmorGuantleft[graphic].icon;
            }
            // right gaunt
            if ((int)currentCreation.equipSlot == 8)
            {
                int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.metalArmorGuantright.Count);

                currentCreation.graphicId = EquipmentRepo._instance.metalArmorGuantright[graphic].id;

                currentCreation.characterVisibleSprite = EquipmentRepo._instance.metalArmorGuantright[graphic].visible;
                currentCreation.MyIcon = EquipmentRepo._instance.metalArmorGuantright[graphic].icon;
            }
            // shoulder
            if ((int)currentCreation.equipSlot == 9)
            {
                int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.metalArmorShoulder.Count);

                currentCreation.graphicId = EquipmentRepo._instance.metalArmorShoulder[graphic].id;

                currentCreation.characterVisibleSprite = EquipmentRepo._instance.metalArmorShoulder[graphic].visible;
                currentCreation.MyIcon = EquipmentRepo._instance.metalArmorShoulder[graphic].icon;
            }
        }
        // magical
        else if (currentCreation.armorType == ArmorType.Magical)
        {

        }

        if ((int)currentCreation.MyQuality == 0)
        {
            currentCreation.damageModifier = 0;
            currentCreation.armorModifier = UnityEngine.Random.Range(2, (playerLvl)) / (1 + (int)currentCreation.equipSlot);

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 10), (playerLvl + 10 * 2));
        }

        else if ((int)currentCreation.MyQuality == 1)
        {
            currentCreation.damageModifier = 0;
            currentCreation.armorModifier = UnityEngine.Random.Range(4, (playerLvl + 2)) / (1 +(int)currentCreation.equipSlot);

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 10), (playerLvl + 10 * 2));
        }

        else if ((int)currentCreation.MyQuality == 2)
        {
            currentCreation.damageModifier = 0;
            currentCreation.armorModifier = UnityEngine.Random.Range(6, (playerLvl + 4)) / (1 + (int)currentCreation.equipSlot);

            currentCreation.sta = UnityEngine.Random.Range(0, (playerLvl / 4));
            currentCreation.str = UnityEngine.Random.Range(0, (playerLvl / 4));
            currentCreation.agi = UnityEngine.Random.Range(0, (playerLvl / 4));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 10), (playerLvl + 10 * 2));
        }

        else if ((int)currentCreation.MyQuality == 3)
        {
            currentCreation.damageModifier = UnityEngine.Random.Range(0, (playerLvl / 2));
            currentCreation.armorModifier = UnityEngine.Random.Range(10, (playerLvl + 10)) / (1 + (int)currentCreation.equipSlot);

            currentCreation.sta = UnityEngine.Random.Range((playerLvl / 4), (playerLvl / 2));
            currentCreation.str = UnityEngine.Random.Range((playerLvl / 4), (playerLvl / 2));
            currentCreation.agi = UnityEngine.Random.Range((playerLvl / 4), (playerLvl / 2));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 10), (playerLvl + 10 * 2));
        }

        else if ((int)currentCreation.MyQuality == 4)
        {
            currentCreation.damageModifier = UnityEngine.Random.Range(2, (playerLvl));
            currentCreation.armorModifier = UnityEngine.Random.Range(15, (playerLvl + 25)) / (1 + (int)currentCreation.equipSlot);

            currentCreation.sta = UnityEngine.Random.Range((playerLvl / 2), (playerLvl));
            currentCreation.str = UnityEngine.Random.Range((playerLvl / 2), (playerLvl));
            currentCreation.agi = UnityEngine.Random.Range((playerLvl / 2), (playerLvl));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 10), (playerLvl + 10 * 2));
        }

        string currentName = EquipNameGenerator._instance.GetArmorName(currentCreation.MyQuality, material, currentCreation.equipSlot);
        currentCreation.MyTitle = currentName;
        currentCreation.name = currentName;

        currentCreation.buyValue = currentCreation.sellValue * 6;
    }

    /// <summary>
    /// Creates a sword for the currentCreation
    /// </summary>
    /// <param name="currentCreation">Current item under creation</param>
    /// <param name="playerLvl">Current Player level</param>
    private void CreateSword(Equipment currentCreation, int playerLvl)
    {
        currentCreation.equipType = 0;
        currentCreation.typeOfEquipment = 0;
        currentCreation.equipSlot = EquipmentSlot.MainHand;

        currentCreation.MyQuality = DetermineRarity();

        if ((int)currentCreation.MyQuality == 0)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.swords.Count);

            currentCreation.graphicId = EquipmentRepo._instance.swords[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.swords[graphic].visible;
            currentCreation.MyGlowSprite = EquipmentRepo._instance.swords[graphic].glow;
            currentCreation.MyIcon = EquipmentRepo._instance.swords[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(1, playerLvl + 3);
            currentCreation.armorModifier = UnityEngine.Random.Range(0, (playerLvl / 10));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 10), (playerLvl + 10 * 2));
        }

        else if ((int)currentCreation.MyQuality == 1)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.swords.Count);

            currentCreation.graphicId = EquipmentRepo._instance.swords[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.swords[graphic].visible;
            currentCreation.MyGlowSprite = EquipmentRepo._instance.swords[graphic].glow;
            currentCreation.MyIcon = EquipmentRepo._instance.swords[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(playerLvl + 3, playerLvl + 5);
            currentCreation.armorModifier = UnityEngine.Random.Range(0, (playerLvl / 6));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 15), (playerLvl + 15 * 2));
        }

        else if ((int)currentCreation.MyQuality == 2)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.swords.Count);

            currentCreation.graphicId = EquipmentRepo._instance.swords[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.swords[graphic].visible;
            currentCreation.MyGlowSprite = EquipmentRepo._instance.swords[graphic].glow;
            currentCreation.MyIcon = EquipmentRepo._instance.swords[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(playerLvl + 5, playerLvl + 10);
            currentCreation.armorModifier = UnityEngine.Random.Range(0, (playerLvl / 2));

            currentCreation.sta = UnityEngine.Random.Range(0, (playerLvl / 4));
            currentCreation.str = UnityEngine.Random.Range(0, (playerLvl / 4));
            currentCreation.agi = UnityEngine.Random.Range(0, (playerLvl / 4));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 35), (playerLvl + 35 * 2));
        }

        else if ((int)currentCreation.MyQuality == 3)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.swords.Count);

            currentCreation.graphicId = EquipmentRepo._instance.swords[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.swords[graphic].visible;
            currentCreation.MyGlowSprite = EquipmentRepo._instance.swords[graphic].glow;
            currentCreation.MyIcon = EquipmentRepo._instance.swords[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(playerLvl + 10, playerLvl + 15);
            currentCreation.armorModifier = UnityEngine.Random.Range(0, (playerLvl));

            currentCreation.sta = UnityEngine.Random.Range(0, (playerLvl / 2));
            currentCreation.str = UnityEngine.Random.Range(0, (playerLvl / 2));
            currentCreation.agi = UnityEngine.Random.Range(0, (playerLvl / 2));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 113), (playerLvl + 113 * 2));
        }

        else if ((int)currentCreation.MyQuality == 4)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.swords.Count);

            currentCreation.graphicId = EquipmentRepo._instance.swords[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.swords[graphic].visible;
            currentCreation.MyGlowSprite = EquipmentRepo._instance.swords[graphic].glow;
            currentCreation.MyIcon = EquipmentRepo._instance.swords[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(playerLvl + 15, playerLvl + 25);
            currentCreation.armorModifier = UnityEngine.Random.Range(0, (playerLvl * 2));

            currentCreation.sta = UnityEngine.Random.Range((playerLvl / 2), (playerLvl));
            currentCreation.str = UnityEngine.Random.Range((playerLvl / 2), (playerLvl));
            currentCreation.agi = UnityEngine.Random.Range((playerLvl / 2), (playerLvl));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 350), (playerLvl + 350 * 4));
        }

        currentCreation.buyValue = currentCreation.sellValue * 6;


        string currentName = EquipNameGenerator._instance.GetSwordName(currentCreation.MyQuality);
        currentCreation.MyTitle = currentName;
        currentCreation.name = currentName;
    }

    /// <summary>
    /// Creates a sword for the currentCreation
    /// </summary>
    /// <param name="currentCreation">Current item under creation</param>
    /// <param name="playerLvl">Current Player level</param>
    private void CreateShield(Equipment currentCreation, int playerLvl)
    {
        currentCreation.equipType = EquipmentType.Armor;
        currentCreation.typeOfEquipment = EquipmentType.Armor;
        currentCreation.equipSlot = EquipmentSlot.OffHand;

        currentCreation.MyQuality = DetermineRarity();

        if ((int)currentCreation.MyQuality == 0)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.shields.Count);

            currentCreation.graphicId = EquipmentRepo._instance.shields[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.shields[graphic].visible;
            currentCreation.MyIcon = EquipmentRepo._instance.shields[graphic].icon;

            currentCreation.damageModifier = 0;
            currentCreation.armorModifier = UnityEngine.Random.Range(playerLvl, (playerLvl + 3));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 10), (playerLvl + 10 * 2));
        }

        else if ((int)currentCreation.MyQuality == 1)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.shields.Count);

            currentCreation.graphicId = EquipmentRepo._instance.shields[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.shields[graphic].visible;
            currentCreation.MyIcon = EquipmentRepo._instance.shields[graphic].icon;

            currentCreation.damageModifier = 0;
            currentCreation.armorModifier = UnityEngine.Random.Range(playerLvl + 1, (playerLvl + 6));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 15), (playerLvl + 15 * 2));
        }

        else if ((int)currentCreation.MyQuality == 2)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.shields.Count);

            currentCreation.graphicId = EquipmentRepo._instance.shields[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.shields[graphic].visible;
            currentCreation.MyIcon = EquipmentRepo._instance.shields[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(0, playerLvl / 4);
            currentCreation.armorModifier = UnityEngine.Random.Range(playerLvl + 4, (playerLvl + 10));

            currentCreation.sta = UnityEngine.Random.Range(0, (playerLvl / 4));
            currentCreation.str = UnityEngine.Random.Range(0, (playerLvl / 4));
            currentCreation.agi = UnityEngine.Random.Range(0, (playerLvl / 4));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 35), (playerLvl + 35 * 2));
        }

        else if ((int)currentCreation.MyQuality == 3)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.shields.Count);

            currentCreation.graphicId = EquipmentRepo._instance.shields[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.shields[graphic].visible;
            currentCreation.MyIcon = EquipmentRepo._instance.shields[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(playerLvl / 4, playerLvl / 2);
            currentCreation.armorModifier = UnityEngine.Random.Range(playerLvl + 4, (playerLvl + 10));

            currentCreation.sta = UnityEngine.Random.Range(0, (playerLvl / 2));
            currentCreation.str = UnityEngine.Random.Range(0, (playerLvl / 2));
            currentCreation.agi = UnityEngine.Random.Range(0, (playerLvl / 2));

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 113), (playerLvl + 113 * 2));
        }

        else if ((int)currentCreation.MyQuality == 4)
        {
            int graphic = UnityEngine.Random.Range(0, EquipmentRepo._instance.shields.Count);

            currentCreation.graphicId = EquipmentRepo._instance.shields[graphic].id;

            currentCreation.characterVisibleSprite = EquipmentRepo._instance.shields[graphic].visible;
            currentCreation.MyIcon = EquipmentRepo._instance.shields[graphic].icon;

            currentCreation.damageModifier = UnityEngine.Random.Range(playerLvl / 2, playerLvl);
            currentCreation.armorModifier = UnityEngine.Random.Range(playerLvl + 8, (playerLvl + 20));

            currentCreation.sta = UnityEngine.Random.Range((playerLvl / 2), playerLvl);
            currentCreation.str = UnityEngine.Random.Range((playerLvl / 2), playerLvl);
            currentCreation.agi = UnityEngine.Random.Range((playerLvl / 2), playerLvl);

            currentCreation.sellValue = UnityEngine.Random.Range((playerLvl + 350), (playerLvl + 350 * 4));
        }

        currentCreation.buyValue = currentCreation.sellValue * 6;


        string currentName = EquipNameGenerator._instance.GetShieldName(currentCreation.MyQuality);
        currentCreation.MyTitle = currentName;
        currentCreation.name = currentName;
    }

    /// <summary>
    /// Determines which slot the armor under creation belongs to
    /// </summary>
    /// <returns></returns>
    EquipmentSlot DetermineArmorSlot()
    {
        int randSlot = UnityEngine.Random.Range(0, 8);

        switch (randSlot)
        {
            case 0:
                return EquipmentSlot.Head;
            case 1:
                return EquipmentSlot.Chest;
            case 2:
                return EquipmentSlot.Legs;
            case 3:
                return EquipmentSlot.FrontFoot;
            case 4:
                return EquipmentSlot.BackFoot;
            case 5:
                return EquipmentSlot.GauntletLeft;
            case 6:
                return EquipmentSlot.GauntletRight;
            case 7:
                return EquipmentSlot.Shoulder;
        }

        Debug.Log("Reached end of switch case which I shouldnt");
        return EquipmentSlot.Chest;
    }

    /// <summary>
    /// Determines the quality of the item being created
    /// </summary>
    /// <returns></returns>
    Quality DetermineRarity()
    {
        float n = UnityEngine.Random.Range(0.0f, 100.0f);

        if (n <= 60)
        {
            return Quality.Common;
        }
        else if (n > 60 && n <= 80)
        {
            return Quality.UnCommon;
        }
        else if (n > 80 && n <= 95)
        {
            return Quality.Rare;
        }
        else if (n > 95 && n <= 99.5)
        {
            return Quality.Epic;
        }
        else if (n > 99.5)
        {
            Debug.LogWarning("LEGENDARY!!!");
            return Quality.Legendary;
        }

        return Quality.Common;

    }
}
