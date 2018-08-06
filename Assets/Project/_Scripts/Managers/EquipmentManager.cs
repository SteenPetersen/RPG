using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour {

    #region Singleton

    public static EquipmentManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion

    public delegate void OnEquipmentChanged(Equipment newItem, Equipment oldItem);
    public OnEquipmentChanged onEquipmentChanged;

    public SpriteRenderer[] visibleGear = new SpriteRenderer[10];
    public EquipedItemSlot[] inventoryEquipment;

    /// <summary>
    /// Glow slot on player - used to set the glow effect for weapons that have one
    /// Playercontroller needs access to it while it charges hits
    /// </summary>
    public SpriteRenderer weaponGlowSlot;

    Sprite[] startGraphics;
    ProjectileList listOfProjectiles;
    Sprite targetSprite;

    // currently equipped items
    public List<Equipment> currentEquipment = new List<Equipment>(13);

    InventoryScript inventory;
    PlayerController player;

    private void Start()
    {
        inventory = InventoryScript.instance;
        int numberOfSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        //currentEquipment = new List<Equipment>(numberOfSlots);

        startGraphics = new Sprite[visibleGear.Length];
        SetVisibleGearSpriteRenderers();
        checkStarterGraphics();


        player = PlayerController.instance;
        listOfProjectiles = ProjectileList.instance;
    }

    public bool Equip (Equipment newItem)
    {
        //Debug.Log("calling equip");

        // find slotIndex of the new item.
        int slotIndex = (int)newItem.equipSlot;

        // if Item has a glow effect add it here
        if (newItem.MyGlowSprite != null)
        {
            weaponGlowSlot.sprite = newItem.MyGlowSprite;
        }

        // instantiate oldItem
        Equipment oldItem = null;

        // if newItem is a bow
        if (slotIndex == 3 && (int)newItem.equipType == 1)
        {
            // is there something in offhand?
            if (currentEquipment[4] != null)
            {
                var oldOffhand = currentEquipment[4];
                oldItem = currentEquipment[3];

                // if there is space for the item in the inventory
                if (inventory.MyEmptySlotCount >= 2)
                {
                    PlaceItemsInInventory(newItem, slotIndex, oldItem, oldOffhand);
                }

                else if (inventory.MyEmptySlotCount <= 1)
                {
                    return false;
                }
            }

            //great! nothing in offhand
            else if (currentEquipment[4] == null)
            {
                if (currentEquipment[3] != null)
                {
                    oldItem = currentEquipment[3];

                    if (inventory.MyEmptySlotCount >= 1)
                    {
                        inventory.AddItem(oldItem);
                        UpdateEquipmentSlot(newItem, slotIndex);
                    }

                    else if (inventory.MyEmptySlotCount == 0)
                    {
                        return false;
                    }
                }

                //else if (currentEquipment[3] == null)
                //{
                //    UpdateEquipmentSlot(newItem, slotIndex);
                //}

            }
        }

        // if its a shield
        if (slotIndex == 4 && (int)newItem.equipType == 2)
        {
            // check if a bow is equipped
            if (currentEquipment[3] != null)
            {
                if ((int)currentEquipment[3].equipType == 1)
                {
                    return false;
                }
            }
        }

        //if its not a bow but you have an item in that position
        if (currentEquipment[slotIndex] != null && (int)newItem.equipType != 1)
        {
            // is the item in that position a bow?
            if ((int)currentEquipment[slotIndex].equipType == 1)
            {
                // remove the arrow
                visibleGear[4].sprite = null;
            }

            oldItem = currentEquipment[slotIndex];

            if (inventory.MyEmptySlotCount >= 1)
            {
                inventory.AddItem(oldItem);
                UpdateEquipmentSlot(newItem, slotIndex);
                //onEquipmentChanged(newItem, oldItem);
            }
            else if (inventory.MyEmptySlotCount == 0)
            {
                return false;
            }
        }

        // invoke the callback for change of equipment
        if (onEquipmentChanged != null)
        {
            onEquipmentChanged.Invoke(newItem, oldItem);
        }

        inventoryEquipment[slotIndex].AddItem(newItem);
        UpdateEquipmentSlot(newItem, slotIndex);
        //Debug.Log("finished calling equip");
        return true;

    }

    private void PlaceItemsInInventory(Equipment newItem, int slotIndex, Equipment oldItem, Equipment oldOffhand)
    {
        // add the two items that the player was holding to the inventory
        inventory.AddItem(oldItem);
        inventory.AddItem(oldOffhand);

        //equip at offhand slot is null
        currentEquipment[4] = null;

        // set the sprite of the slot to the start sprite of the game
        visibleGear[4].sprite = startGraphics[4];

        // remove that item from the list of currently equipped gear.
        ClearEquippedGear(4);

        // update the array of currently equipped items with the new Item

        UpdateEquipmentSlot(newItem, slotIndex);

        //onEquipmentChanged(newItem, oldItem);

        //onEquipmentChanged(null, oldOffhand);
    }

    /// <summary>
    /// Checks to see if the slot that the item is trying to be equipped in matches the type of Item
    /// </summary>
    /// <param name="slotIndex">slot to try and match with</param>
    /// <param name="item">item to compare to</param>
    /// <returns></returns>
    public bool CheckIfSlotMatchesType(int slotIndex, Equipment item)
    {
        if ((int)item.equipSlot == slotIndex)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates the gear that the player is wearing and then calls a function to update the slots with the correct information
    /// </summary>
    /// <param name="newItem">Item to wear</param>
    /// <param name="slotIndex">Slot to update</param>
    private void UpdateEquipmentSlot(Equipment newItem, int slotIndex)
    {
        // update the array of equipment with the new items
        currentEquipment[slotIndex] = newItem;

        //Debug.Log(slotIndex);

        // instantiate a sprite corresponding to the new items visiblesprite
        //Sprite newSprite = Instantiate(newItem.characterVisibleSprite) as Sprite;

        // update the sprite of the equipmentSlots with the new sprite
        visibleGear[slotIndex].sprite = newItem.characterVisibleSprite;

        // Make sure all slots have the correct Icon and that they know what item they are holding
        UpdateSlots();
    }

    public void Unequip (int slotIndex)
    {
        // check that you have something in the slot
        if (currentEquipment[slotIndex] != null)
        {
            //trying to unequip a bow
            if ((int)currentEquipment[slotIndex].equipType == 1)
            {

                if (inventory.MyEmptySlotCount >= 1)
                {
                    //remove the arrow from the players hand
                    visibleGear[4].sprite = null;
                }
                else if (inventory.MyEmptySlotCount == 0)
                {
                    return;
                }
            }

            //keep a reference to the item that was in the slot
            Equipment oldItem = currentEquipment[slotIndex];
            // check to see if there is space in the bags for unequipping the item. if there is space then add it
            bool spaceAvailable = inventory.AddItem(oldItem);

            // if there isnt space
            if (!spaceAvailable)
            {
                return;
            }

            //remove the item from the current slot
            currentEquipment[slotIndex] = null;

            // set the sprite of the slot to the start sprite of the game
            visibleGear[slotIndex].sprite = startGraphics[slotIndex];
            // remove that item from the list of currently equipped gear.
            ClearEquippedGear(slotIndex);

            // if the items if the main hand item.
            if (slotIndex == 3)
            {
                player.melee = true;
                player.ranged = false;
            }

            if (onEquipmentChanged != null)
            {
                onEquipmentChanged.Invoke(null, oldItem);
            }
        }
    }

    public void UnequipByDragging(int slotIndex)
    {
        // check that you have something in the slot
        if (currentEquipment[slotIndex] != null)
        {
            //trying to unequip a bow
            if ((int)currentEquipment[slotIndex].equipType == 1)
            {
                //remove the arrow from the players hand
                visibleGear[4].sprite = null;
            }

            //keep a reference to the item that was in the slot
            Equipment oldItem = currentEquipment[slotIndex];

            //remove the item from the current slot
            currentEquipment[slotIndex] = null;

            // set the sprite of the slot to the start sprite of the game
            visibleGear[slotIndex].sprite = startGraphics[slotIndex];

            // remove that item from the list of currently equipped gear.
            ClearEquippedGear(slotIndex);

            // if the item is the main hand item.
            if (slotIndex == 3)
            {
                player.melee = true;
                player.ranged = false;
            }

            if (onEquipmentChanged != null)
            {
                onEquipmentChanged.Invoke(null, oldItem);
            }
        }
    }

    public void UnequipAll()
    {
        for (int i = 0; i < currentEquipment.Count; i++)
        {
            Unequip(i);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UnequipAll();
        }

        if (player == null)
        {
            player = PlayerController.instance;
        }
    }

    void SetVisibleGearSpriteRenderers()
    {
        for (int i = 0; i < 10; i++)
        {
            if (i == 0)
            {
                visibleGear[i] = PlayerController.instance.gameObject.transform.Find("Skeleton/Body/Head/Helm").GetComponent<SpriteRenderer>();
            }
            else if (i == 1)
            {
                visibleGear[i] = PlayerController.instance.gameObject.transform.Find("Skeleton/Body").GetComponent<SpriteRenderer>();
            }
            else if (i == 2)
            {
                visibleGear[i] = PlayerController.instance.gameObject.transform.Find("Skeleton/Legs").GetComponent<SpriteRenderer>();
            }
            else if (i == 3)
            {
                visibleGear[i] = PlayerController.instance.gameObject.transform.Find("Skeleton/Body/MainHand/MainItem").GetComponent<SpriteRenderer>();
            }
            else if (i == 4)
            {
                visibleGear[i] = PlayerController.instance.gameObject.transform.Find("Skeleton/Body/OffHand/OffItem").GetComponent<SpriteRenderer>();
            }
            else if (i == 5)
            {
                visibleGear[i] = PlayerController.instance.gameObject.transform.Find("Skeleton/Legs/MainFoot").GetComponent<SpriteRenderer>();
            }
            else if (i == 6)
            {
                visibleGear[i] = PlayerController.instance.gameObject.transform.Find("Skeleton/Legs/OffFoot").GetComponent<SpriteRenderer>();
            }
            else if (i == 7)
            {
                visibleGear[i] = PlayerController.instance.gameObject.transform.Find("Skeleton/Body/MainHand").GetComponent<SpriteRenderer>();
            }
            else if (i == 8)
            {
                visibleGear[i] = PlayerController.instance.gameObject.transform.Find("Skeleton/Body/OffHand").GetComponent<SpriteRenderer>();
            }
            else if (i == 9)
            {
                visibleGear[i] = PlayerController.instance.gameObject.transform.Find("Skeleton/Body/Shoulder").GetComponent<SpriteRenderer>();
            }
        }
    }

    void checkStarterGraphics()
    {
        for (int i = 0; i < visibleGear.Length; i++)
        {
            if (visibleGear[i] != null)
            {
                startGraphics[i] = visibleGear[i].sprite;
            }
        }
    }

    void UpdateSlots()
    {
        foreach (var equip in currentEquipment)
        {
            if (equip != null)
            {
                inventoryEquipment[(int)equip.equipSlot].MyIcon.sprite = equip.icon;
                inventoryEquipment[(int)equip.equipSlot].MyIcon.enabled = true;
                inventoryEquipment[(int)equip.equipSlot].MyEquipment = equip;

                CheckIfMainHandItemAndSetState(equip);
            }

        }
    }

    /// <summary>
    /// clears the gear at the specified slot index
    /// </summary>
    /// <param name="slotIndex">Slot to be cleared</param>
    void ClearEquippedGear(int slotIndex)
    {
        inventoryEquipment[slotIndex].MyIcon.sprite = null;
        inventoryEquipment[slotIndex].MyIcon.enabled = false;
    }

    private void CheckIfMainHandItemAndSetState(Equipment equip)
    {
        if ((int)equip.equipSlot == 3)
        {
            int stateId = (int)equip.equipType;
            if (stateId == 0)
            {
                player.melee = true;
                player.ranged = false;
            }
            else if (stateId == 1)
            {
                player.melee = false;
                player.ranged = true;
                SetProjectileType(equip.rangedProjectile);
            }
            else
            {
                Debug.Log("this item is labelled wrong " + equip.name);
            }
        }
    }

    public void SetProjectileType(int projectileType)
    {
        //find the type of projectile
        GameObject newProjectile = listOfProjectiles.GetProjectile(projectileType);

        //Debug.Log("Pooling Arrows");

        if (currentEquipment[4] != null)
        {
            var oldItem = currentEquipment[4];
            bool space = inventory.AddItem(oldItem);

            if (!space)
            {
                return;
            }

            currentEquipment[4] = null;
            visibleGear[4].sprite = startGraphics[4];
            ClearEquippedGear(4);
        }

        //replace the currently pooled projectile with the new ones. TODO only do this if its different
        PooledProjectilesController.instance.PopulateArrows(newProjectile);

        // change graphic in the hand of the player, and remove shield if equipped
        Sprite newSprite = Instantiate<Sprite>(newProjectile.GetComponent<SpriteRenderer>().sprite);
        visibleGear[4].sprite = newSprite;

    }
}
