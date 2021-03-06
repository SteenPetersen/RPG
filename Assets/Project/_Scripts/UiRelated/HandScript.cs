﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class HandScript : MonoBehaviour {

    public static HandScript instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    public IMoveable MyMoveable { get; set; }

    private Image icon;

    [SerializeField]
    private Vector3 offSet;

    void Start () {
        icon = GetComponent<Image>();
	}
	
	void Update () {
        icon.transform.position = Input.mousePosition + offSet;

        DeleteItem();
	}

    public void TakeMoveable(IMoveable moveable)
    {
        if (moveable != null)
        {
            this.MyMoveable = moveable;
            icon.sprite = moveable.MyIcon;
            icon.color = Color.white;
        }
    }

    public void Drop()
    {
        MyMoveable = null;
        icon.color = new Color(0, 0, 0, 0);
    }

    private void DeleteItem()
    {
        // if I press the mouse button and my mouse isnt over UI element and I have something in my hand
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && instance.MyMoveable != null)
        {
            if (MyMoveable is Key)
                return;

            // If its and item in my hand from the inventory
            if (MyMoveable is Item && InventoryScript.instance.FromSlot != null)
            {
                (MyMoveable as Item).MySlot.Clear();
            }

            Drop();

            InventoryScript.instance.FromSlot = null;
        }
    }

    public IMoveable Put()
    {
        IMoveable tmp = MyMoveable;

        MyMoveable = null;

        icon.color = new Color(0, 0, 0, 0);

        return tmp;
    }
}
