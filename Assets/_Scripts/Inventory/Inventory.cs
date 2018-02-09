using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    #region Singleton
    public static Inventory instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than 1 inventory exists");
            return;
        }

        instance = this;
    }
    #endregion

    // delegate is an event that you can subscribe different methods to
    public delegate void OnItemChanged();
    public OnItemChanged OnItemChangedCallBack;

    public int space = 20;


    public List<Item> itemsInBag = new List<Item>();
    public List<Item> equip = new List<Item>();

    public bool AddItemToBag(Item item)
    {
        if (!item.isDefaultItem)
        {
            if (itemsInBag.Count >= space)
            {
                Debug.Log("not enough room");
                return false;
            }
            itemsInBag.Add(item);

            if (OnItemChangedCallBack != null)
            {
                OnItemChangedCallBack.Invoke();
            }
        }

        return true;
    }

    public void Remove(Item item)
    {
        itemsInBag.Remove(item);


        if (OnItemChangedCallBack != null)
        {
            OnItemChangedCallBack.Invoke();
        }
    }
}
