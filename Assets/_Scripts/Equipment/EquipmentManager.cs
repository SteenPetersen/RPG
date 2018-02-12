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

    public SpriteRenderer[] equipmentSlots;
    public EquipedItemSlot[] inventoryEquipment;


    Sprite[] startGraphics;
    ProjectileList listOfProjectiles;
    Sprite targetSprite;

    // currently equipped items
    public List<Equipment> currentEquipment = new List<Equipment>(13);

    Inventory inventory;
    PlayerController player;

    private void Start()
    {
        inventory = Inventory.instance;
        int numberOfSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        //currentEquipment = new List<Equipment>(numberOfSlots);

        startGraphics = new Sprite[equipmentSlots.Length];

        checkStarterGraphics();
        player = PlayerController.instance;
        listOfProjectiles = ProjectileList.instance;
    }

    public bool Equip (Equipment newItem)
    {
        // find slotIndex of the new item.
        int slotIndex = (int)newItem.equipSlot;

        // instatiate oldItem
        Equipment oldItem = null;

        // if its a bow
        if (slotIndex == 3 && (int)newItem.equipType == 1)
        {
            // is there something in offhand?
            if (currentEquipment[4] != null)
            {
                var oldOffhand = currentEquipment[4];
                oldItem = currentEquipment[3];

                bool space = inventory.CheckIfTwoItemsFitInBag();

                if (space)
                {
                    inventory.AddItemToBag(oldItem);
                    inventory.AddItemToBag(oldOffhand);

                    //equip at offhand slot is null
                    currentEquipment[4] = null;

                    // set the sprite of the slot to the start sprite of the game
                    equipmentSlots[4].sprite = startGraphics[4];
                    // remove that item from the list of currently equipped gear.
                    ClearEquippedGear(4);

                    PlaceTheNewItem(newItem, slotIndex);
                }

                else if (!space)
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
                    bool space = inventory.CheckIfItemsFitInBag();

                    if (space)
                    {
                        inventory.AddItemToBag(oldItem);
                        PlaceTheNewItem(newItem, slotIndex);
                    }

                    else if (!space)
                    {
                        return false;
                    }
                }
                else if (currentEquipment[3] == null)
                {
                    PlaceTheNewItem(newItem, slotIndex);
                }

            }
            return true;

        }

        //if its not a bow but you have an item in that position
        else if (currentEquipment[slotIndex] != null)
        {
            // is the item in that position a bow?
            if ((int)currentEquipment[slotIndex].equipType == 1)
            {
                equipmentSlots[4].sprite = null;
            }

            oldItem = currentEquipment[slotIndex];
            bool space = inventory.CheckIfItemsFitInBag();
            //bool space = inventory.AddItemToBag(oldItem);
            if (space)
            {
                inventory.AddItemToBag(oldItem);
                PlaceTheNewItem(newItem, slotIndex);
            }
            else if (!space)
            {
                return false;
            }

            return true;
        }

        // invoke the callback for change of equopment
        if (onEquipmentChanged != null)
        {
            onEquipmentChanged.Invoke(newItem, oldItem);
        }

        PlaceTheNewItem(newItem, slotIndex);
        return true;

    }

    private void PlaceTheNewItem(Equipment newItem, int slotIndex)
    {
        // update the array of equipment with the new items
        currentEquipment[slotIndex] = newItem;
        // instantiate a sprite corresponding to the new items visiblesprite
        Sprite newSprite = Instantiate<Sprite>(newItem.characterVisibleSprite);
        // update the sprite of the equipmentSlots with the new sprite
        equipmentSlots[slotIndex].sprite = newSprite;
        //loop through all equipment in currentequipment and set inventoryequipmentSlot Icon correctly
        PlaceGearInCorrectSlot();
    }

    public void Unequip (int slotIndex)
    {
        // check that you have something in the slot
        if (currentEquipment[slotIndex] != null)
        {
            //trying to unequip a bow
            if ((int)currentEquipment[slotIndex].equipType == 1)
            {
                bool space = inventory.CheckIfItemsFitInBag();

                if (space)
                {
                    //remove the arrow from the players hand
                    equipmentSlots[4].sprite = null;
                }
                else if (!space)
                {
                    return;
                }
            }

            //keep a reference to the item that was in the slot
            Equipment oldItem = currentEquipment[slotIndex];
            // check to see if there is space in the bags for unequipping the item. if there is space then add it
            bool spaceAvailable = inventory.AddItemToBag(oldItem);

            // if there isnt space
            if (!spaceAvailable)
            {
                return;
            }

            //remove the item from the current slot
            currentEquipment[slotIndex] = null;

            // set the sprite of the slot to the start sprite of the game
            equipmentSlots[slotIndex].sprite = startGraphics[slotIndex];
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

    void checkStarterGraphics()
    {
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            startGraphics[i] = equipmentSlots[i].sprite;
        }
    }

    void PlaceGearInCorrectSlot()
    {
        foreach (var equip in currentEquipment)
        {
            if (equip != null)
            {
                inventoryEquipment[(int)equip.equipSlot].icon.sprite = equip.icon;
                inventoryEquipment[(int)equip.equipSlot].icon.enabled = true;

                CheckIfMainHandItemAndSetState(equip);

            }

        }
    }

    void ClearEquippedGear(int slotIndex)
    {
        inventoryEquipment[slotIndex].icon.sprite = null;
        inventoryEquipment[slotIndex].icon.enabled = false;
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

    private void SetProjectileType(int projectileType)
    {
        //find the type of projectile
        GameObject newProjectile = listOfProjectiles.GetProjectile(projectileType);


        if (currentEquipment[4] != null)
        {
            var oldItem = currentEquipment[4];
            bool space = inventory.AddItemToBag(oldItem);

            if (!space)
            {

                return;
            }

            currentEquipment[4] = null;
            equipmentSlots[4].sprite = startGraphics[4];
            ClearEquippedGear(4);
            //// invoke the callback for change of equopment
            //if (onEquipmentChanged != null)
            //{
            //    onEquipmentChanged.Invoke(null, oldItem);
            //}
        }



        //replace the currently pooled projectile with the new ones. TODO only do this if its different
        PooledProjectilesController.instance.ReplaceArrows(newProjectile);
        // change graphic in the hand of the player, and remove shield if equipped


        Sprite newSprite = Instantiate<Sprite>(newProjectile.GetComponent<SpriteRenderer>().sprite);
        equipmentSlots[4].sprite = newSprite;

    }
}
