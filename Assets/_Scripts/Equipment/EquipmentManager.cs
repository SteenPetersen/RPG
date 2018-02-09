using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour {

    #region Singleton

    public static EquipmentManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than 1 EquipmentManager exists");
            return;
        }

        instance = this;
    }
    #endregion

    public delegate void OnEquipmentChanged(Equipment newItem, Equipment oldItem);
    public OnEquipmentChanged onEquipmentChanged;

    public SpriteRenderer[] equipmentSlots;


    public EquipedItemSlot[] inventoryEquipment;


    Sprite[] startGraphics;

    Sprite targetSprite;
    Equipment[] currentEquipment;

    Inventory inventory;

    private void Start()
    {
        inventory = Inventory.instance;
        int numberOfSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        currentEquipment = new Equipment[numberOfSlots];

        startGraphics = new Sprite[equipmentSlots.Length];

        checkStarterGraphics();
    }

    public void Equip (Equipment newItem)
    {
        // find slotIndex of the new item.
        int slotIndex = (int)newItem.equipSlot;

        // instatiate oldItem
        Equipment oldItem = null;


        // if you have an item at that position add it to the bag.
        if (currentEquipment[slotIndex] != null)
        {
            oldItem = currentEquipment[slotIndex];
            inventory.AddItemToBag(oldItem);
        }

        // invoke the callback for change of equopment
        if (onEquipmentChanged != null)
        {
            onEquipmentChanged.Invoke(newItem, oldItem);
        }

        // place new gear update here in inventory
        currentEquipment[slotIndex] = newItem;
        Sprite newSprite = Instantiate<Sprite>(newItem.characterVisibleSprite);
        equipmentSlots[slotIndex].sprite = newSprite;
        PlaceGearInCorrectSlot(newItem);
    }

    public void Unequip (int slotIndex)
    {
        if (currentEquipment[slotIndex] != null)
        {
            Equipment oldItem = currentEquipment[slotIndex];
            inventory.AddItemToBag(oldItem);

            currentEquipment[slotIndex] = null;

            equipmentSlots[slotIndex].sprite = startGraphics[slotIndex];

            if (onEquipmentChanged != null)
            {
                onEquipmentChanged.Invoke(null, oldItem);
            }
        }
    }

    public void UnequipAll()
    {
        for (int i = 0; i < currentEquipment.Length; i++)
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


    }

    void checkStarterGraphics()
    {
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            startGraphics[i] = equipmentSlots[i].sprite;
        }
    }

    void PlaceGearInCorrectSlot(Equipment newItem)
    {
        foreach (var equip in currentEquipment)
        {
            if (equip != null)
            {
                Debug.Log("woop");
                inventoryEquipment[(int)equip.equipSlot].icon.sprite = equip.icon;
                inventoryEquipment[(int)equip.equipSlot].icon.enabled = true;
            }
        }
    }

}
