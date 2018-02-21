using UnityEngine;

public class InventoryUI : MonoBehaviour {

    public Transform itemsParent;
    public GameObject BagUI;

    public GameObject inventoryCam;

    Inventory inventory;

    InventorySlot[] slots;

	void Start () {
        inventory = Inventory.instance;
        inventory.OnItemChangedCallBack += UpdateUI;

        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
	}
	
	void Update () {
        //if (Input.GetButtonDown("Bag"))
        //{
        //    BagUI.SetActive(!BagUI.activeSelf);
        //}

        if (Input.GetButtonDown("Inventory"))
        {
            BagUI.SetActive(!BagUI.activeSelf);
            inventoryCam.SetActive(!inventoryCam.activeSelf);
        }

    }

    void UpdateUI()
    {
        //Debug.Log("updating UI");
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.itemsInBag.Count)
            {
                slots[i].AddItem(inventory.itemsInBag[i]);
            }
            else
            {
                slots[i].ClearSLot();
            }
        }
    }

    private void OnDisable()
    {
        inventory = Inventory.instance;
        inventory.OnItemChangedCallBack -= UpdateUI;
    }

    //void UpdateEquipment()
    //{
    //    Debug.Log("updating Equipment");
    //    for (int i = 0; i < equipedSlots.Length; i++)
    //    {
    //        if (i < inventory.itemsInBag.Count)
    //        {
    //            slots[i].AddItem(inventory.itemsInBag[i]);
    //        }
    //        else
    //        {
    //            slots[i].ClearSLot();
    //        }
    //    }
    //}
}
