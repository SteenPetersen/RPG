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
    PlayerStats stats;

    private void Start()
    {
        inventory = InventoryScript.instance;
        int numberOfSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        //currentEquipment = new List<Equipment>(numberOfSlots);

        startGraphics = new Sprite[visibleGear.Length];
        SetVisibleGearSpriteRenderers();
        checkStarterGraphics();

        player = PlayerController.instance;
        stats = PlayerStats.instance;

        weaponGlowSlot = player.transform.Find("Skeleton/Body/MainHand/MainItemGlow").GetComponent<SpriteRenderer>();

        listOfProjectiles = ProjectileList.instance;
    }

    public bool Equip (Equipment newStatsToAdd, bool silent = false)
    {
        if (VendorManager.instance.vendorWindowOpen)
            return false;

        int slotIndex = (int)newStatsToAdd.equipSlot;

        /// These item are set to null to start with because we invoke a callback that
        /// will remove the stats from olditem and add the stats on newitem. only populate them 
        /// if we are about the replace an item.
        Equipment oldStatsToRemove = null;
        Equipment oldOffhand = null;



        // newItem = Bow
        if (slotIndex == 3 && (int)newStatsToAdd.equipType == 1)
        {
            if (DebugControl.debugInventory)
            {
                Debug.Log("Adding Bow as a new item");
            }

            /// if player has items equipped in both main and offhand
            if (currentEquipment[3] != null && currentEquipment[4] != null)
            {
                if (DebugControl.debugInventory)
                {
                    Debug.Log("player has items equipped in both main and offhand");
                }

                oldStatsToRemove = currentEquipment[3];
                oldOffhand = currentEquipment[4];

                stats.RemoveItemStats(oldOffhand);

                /// if there is space for the item in the inventory
                if (inventory.MyEmptySlotCount >= 2)
                {
                    ClearOffhandSlotAndUpdateVisuals(newStatsToAdd, slotIndex);
                    SaveItemInfoAndClearSlot(newStatsToAdd);
                    inventory.AddItemToFirstInvSlot(oldStatsToRemove);
                    inventory.AddItemToSecondInvSlot(oldOffhand);
                }

                else if (inventory.MyEmptySlotCount == 1)
                {
                    if (DebugControl.debugInventory)
                    {
                        Debug.Log("Have only 1 slot available");
                    }

                    SlotScript one = inventory.GetSlotScript(0);
                    SlotScript two = inventory.GetSlotScript(1);

                    SlotInfo firstSlot = SaveItemInfoAndClearSlot(one);
                    SlotInfo secondSlot = SaveItemInfoAndClearSlot(two);

                    inventory.ClearFromSlot(newStatsToAdd.MySlot);

                    inventory.AddItemToFirstInvSlot(oldStatsToRemove);
                    inventory.AddItemToSecondInvSlot(oldOffhand);

                    if (firstSlot.slotItem != null)
                    {
                        inventory.AddItem(firstSlot.slotItem);
                    }

                    if (secondSlot.slotItem != null)
                    {
                        inventory.AddItem(secondSlot.slotItem);
                    }

                    ClearOffhandSlotAndUpdateVisuals(newStatsToAdd, slotIndex);

                }

                else if (inventory.MyEmptySlotCount < 1)
                {
                    if (DebugControl.debugInventory)
                    {
                        Debug.Log("Sending Notice that player cannot perform this action");
                    }


                    var text = CombatTextManager.instance.FetchText(transform.position);
                    var textScript = text.GetComponent<CombatText>();
                    textScript.White("Not enough room!", transform.position);
                    text.transform.position = player.transform.position;
                    text.SetActive(true);
                    textScript.FadeOut();

                    return false;
                }
            }

            /// Player has a main hand equipped but not a shield
            else if (currentEquipment[3] != null && currentEquipment[4] == null)
            {
                if (DebugControl.debugInventory)
                {
                    Debug.Log("Player has a main hand equipped but not a shield");
                }

                oldStatsToRemove = currentEquipment[slotIndex];

                if (inventory.MyEmptySlotCount >= 1)
                {
                    SaveItemInfoAndClearSlot(newStatsToAdd);
                    inventory.AddItemToFirstInvSlot(oldStatsToRemove);
                }

                else if (inventory.MyEmptySlotCount == 0)
                {
                    SaveItemInfoAndClearSlot(newStatsToAdd);
                    inventory.AddItem(oldStatsToRemove);
                }

                ClearOffhandSlotAndUpdateVisuals(newStatsToAdd, slotIndex);

            }

            /// Player does not have a main hand equipped but has a shield
            else if (currentEquipment[3] == null && currentEquipment[4] != null)
            {
                if (DebugControl.debugInventory)
                {
                    Debug.Log("Player does not have a main hand equipped but has a shield");
                }

                oldOffhand = currentEquipment[4];
                stats.RemoveItemStats(oldOffhand);

                if (inventory.MyEmptySlotCount >= 1)
                {
                    SaveItemInfoAndClearSlot(newStatsToAdd);
                    inventory.AddItemToSecondInvSlot(oldOffhand);
                }

                else if (inventory.MyEmptySlotCount == 0)
                {
                    SaveItemInfoAndClearSlot(newStatsToAdd);
                    inventory.AddItem(oldOffhand);
                }

                ClearOffhandSlotAndUpdateVisuals(newStatsToAdd, slotIndex);
            }

            /// Nothing equipped
            else
            {
                SaveItemInfoAndClearSlot(newStatsToAdd);
            }
        }



        // if its a shield
        else if (slotIndex == 4 && (int)newStatsToAdd.equipType == 2)
        {
            /// if player has items equipped in both main and offhand
            if (currentEquipment[3] != null && currentEquipment[4] != null)
            {
                oldStatsToRemove = currentEquipment[4];
                oldOffhand = currentEquipment[4];

                if (inventory.MyEmptySlotCount >= 1)
                {
                    SaveItemInfoAndClearSlot(newStatsToAdd);
                    inventory.AddItemToSecondInvSlot(oldOffhand);
                }

                else if (inventory.MyEmptySlotCount == 0)
                {
                    SaveItemInfoAndClearSlot(newStatsToAdd);
                    inventory.AddItem(oldOffhand);
                }
            }

            /// Player has a main hand equipped but not a shield
            /// It could be a bow or a single melee weapon
            else if (currentEquipment[3] != null && currentEquipment[4] == null)
            {
                ///Its a bow that he has equipped
                if ((int)currentEquipment[3].equipType == 1)
                {
                    if (DebugControl.debugInventory)
                    {
                        Debug.Log("Adding Shield in place of Bow");
                    }

                    oldStatsToRemove = currentEquipment[3];

                    SaveItemInfoAndClearSlot(newStatsToAdd);
                    inventory.AddItem(oldStatsToRemove);
                    ClearMainHandSlot(newStatsToAdd, slotIndex);
                }

                else
                {
                    SaveItemInfoAndClearSlot(newStatsToAdd);
                }
            }

            /// Player does not have a main hand equipped but has a shield
            else if (currentEquipment[3] == null && currentEquipment[4] != null)
            {
                oldStatsToRemove = currentEquipment[slotIndex];
                oldOffhand = currentEquipment[slotIndex];

                if (inventory.MyEmptySlotCount >= 1)
                {
                    SaveItemInfoAndClearSlot(newStatsToAdd);
                    inventory.AddItemToSecondInvSlot(oldOffhand);
                }

                else if (inventory.MyEmptySlotCount == 0)
                {
                    SaveItemInfoAndClearSlot(newStatsToAdd);
                    inventory.AddItem(oldOffhand);
                }
            }

            /// Nothing equipped
            else
            {
                SaveItemInfoAndClearSlot(newStatsToAdd);
            }
        }



        /// if its not a bow but you have an item in that position
        /// Mainly, equipping a melee weapon and all armor
        else if (currentEquipment[slotIndex] != null && (int)newStatsToAdd.equipType != 1)
        {
            /// is the item in that position a bow?
            if ((int)currentEquipment[slotIndex].equipType == 1)
            {
                /// remove the arrow
                visibleGear[4].sprite = null;
            }

            oldStatsToRemove = currentEquipment[slotIndex];

            if (slotIndex == 3)
            {
                SaveItemInfoAndClearSlot(newStatsToAdd);
                inventory.AddItemToFirstInvSlot(oldStatsToRemove);
            }
            else
            {
                SaveItemInfoAndClearSlot(newStatsToAdd);
                inventory.AddItem(oldStatsToRemove);
            }
        }

        /// Adding an item to a slot that has nothing in it
        else if (currentEquipment[slotIndex] == null)
        {
            if (!silent)
            {
                SaveItemInfoAndClearSlot(newStatsToAdd);
            }
        }

        // invoke the callback for change of equipment
        if (onEquipmentChanged != null)
        {
            onEquipmentChanged.Invoke(newStatsToAdd, oldStatsToRemove);
        }



        // if Item has a glow effect add it here
        if (newStatsToAdd.MyGlowSprite != null)
        {
            weaponGlowSlot.sprite = newStatsToAdd.MyGlowSprite;
        }



        // Equip the item and make the sound of equipping
        inventoryEquipment[slotIndex].AddItemVisuals(newStatsToAdd, silent);

        UpdateIteminEquipmentSlot(newStatsToAdd, slotIndex);

        return true;

    }

    private void ClearMainHandSlot(Equipment newItem, int slotIndex)
    {
        /// equip at Mainhand slot is null
        currentEquipment[3] = null;

        /// set the sprite of the slot to the start sprite of the game
        visibleGear[3].sprite = startGraphics[3];

        /// remove that item from the list of currently equipped gear.
        ClearEquippedGear(3);

        /// Clear the glow sprite
        weaponGlowSlot.sprite = null;

        /// update the array of currently equipped items with the new Item
        UpdateIteminEquipmentSlot(newItem, slotIndex);
    }

    private SlotInfo SaveItemInfoAndClearSlot(Equipment newItem)
    {
        SlotScript s = inventory.GetSlotScript(newItem.MySlot);

        if (s != null)
        {
            SlotInfo tmp = new SlotInfo(s.MyCount, s.MyItem);

            s.Clear();

            return tmp;
        }

        return null;
    }

    private SlotInfo SaveItemInfoAndClearSlot(SlotScript slot)
    {
        SlotScript s = inventory.GetSlotScript(slot);

        SlotInfo tmp = new SlotInfo(s.MyCount, s.MyItem);

        s.Clear();

        return tmp;
    }

    private void ClearOffhandSlotAndUpdateVisuals(Equipment newItem, int slotIndex)
    {
        //equip at offhand slot is null
        currentEquipment[4] = null;

        // set the sprite of the slot to the start sprite of the game
        visibleGear[4].sprite = startGraphics[4];

        // remove that item from the list of currently equipped gear.
        ClearEquippedGear(4);

        // update the array of currently equipped items with the new Item
        UpdateIteminEquipmentSlot(newItem, slotIndex);
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
    private void UpdateIteminEquipmentSlot(Equipment newItem, int slotIndex)
    {
        // update the array of equipment with the new items
        currentEquipment[slotIndex] = newItem;

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

public class SlotInfo
{
    public int count;
    public Item slotItem;
    public bool dataAvailable;

    /// <summary>
    /// Constructor for initializing this class
    /// </summary>
    /// <param name="c">How many items</param>
    /// <param name="s">What Item</param>
    public SlotInfo(int c, Item s)
    {
        count = c;
        slotItem = s;
        dataAvailable = true;
    }
}
