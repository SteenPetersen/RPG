﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour {

    public static UiManager instance;

    [SerializeField]
    public GameObject inventoryCam;
    [SerializeField]
    private GameObject equipmentWindow;

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

        keybindButtons = GameObject.FindGameObjectsWithTag("Keybind");
    }

    [SerializeField]
    private ActionButton[] actionButtons;

    [SerializeField]
    private CanvasGroup keybindMenu;

    [SerializeField]
    private GameObject[] keybindButtons;

    // Use this for initialization
    void Start ()
    {
        //SetUseable(actionButtons[0], )
    }

    // Update is called once per frame
    void Update () {

        if (Input.GetKeyDown(KeyCode.B))
        {
            InventoryScript.instance.OpenClose();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            PlayerStats.instance.TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenCloseMenu();
        }

        if (Input.GetButtonDown("Inventory"))
        {
            if (!PlayerController.instance.facingRight)
            {
                PlayerController.instance.FlipPlayer();
            }

            GameDetails.instance.paused = GameDetails.instance.paused == true ? false : true;
            inventoryCam.SetActive(!inventoryCam.activeSelf);
            equipmentWindow.SetActive(!equipmentWindow.activeSelf);

        }

    }

    /// <summary>
    /// Updates the stacksize on a clickable slot
    /// </summary>
    /// <param name="clickable"></param>
    public void UpdateStackSize(IClickable clickable)
    {
        // if the items are stacking
        if (clickable.MyCount > 1)
        {
            clickable.MyStackText.text = clickable.MyCount.ToString();
            clickable.MyStackText.color = Color.white;
            clickable.MyIcon.color = Color.white;
        }
        // when there is only 1 item dont display text
        else
        {
            clickable.MyStackText.color = new Color(0, 0, 0, 0);
            clickable.MyIcon.color = Color.white;
        }

        // if the slot has nothing in it
        if (clickable.MyCount == 0)
        {
            clickable.MyIcon.color = new Color(0, 0, 0, 0);
            clickable.MyStackText.color = new Color(0, 0, 0, 0);
        }
    }

    /// <summary>
    /// Opens and closes the main menu and pauses the game through the gamedetails
    /// </summary>
    public void OpenCloseMenu()
    {
        // if the key bind menus alpha is above zero set it to zero, else set it to 1
        keybindMenu.alpha = keybindMenu.alpha > 0 ? 0 : 1;
        // make sure you can block raycasts
        keybindMenu.blocksRaycasts = keybindMenu.blocksRaycasts == true ? false : true;

        // Tell the gamedetails to pause the game
        GameDetails.instance.paused = GameDetails.instance.paused == true ? false : true;
    }

    /// <summary>
    /// Updates the text in the keybind menu
    /// </summary>
    /// <param name="key">Key for the action or function that is to be changed</param>
    /// <param name="code">The new key that is to be used</param>
    public void UpdateKeyText(string key, KeyCode code)
    {
        // check the array to see if the name of the key is in the array. If it is then grab it's text child
        Text tmp = Array.Find(keybindButtons, x => x.name == key).GetComponentInChildren<Text>();

        // update said text with the correct text
        tmp.text = code.ToString();
    }

    public void ClickActionButton(string buttonName)
    {
        Array.Find(actionButtons, x => x.gameObject.name == buttonName).MyButton.onClick.Invoke();
    }

    public void SetUseable(ActionButton btn, IUseable useable)
    {
        //btn.MyButton.image.sprite = useable.MyIcon;
        //btn.MyButton.image.color = Color.white;
        //btn.MyUseable = useable;
    }


}