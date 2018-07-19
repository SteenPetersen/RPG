using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour {

    public static UiManager instance;

    public GameObject inventoryCam;

    [SerializeField] private GameObject equipmentWindow;

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

        toolTipText = toolTip.GetComponentInChildren<Text>();
        keybindButtons = GameObject.FindGameObjectsWithTag("Keybind");
    }

    [SerializeField] private ActionButton[] actionButtons;

    [SerializeField] private CanvasGroup keybindMenu;

    [SerializeField] private GameObject[] keybindButtons;

    [SerializeField] GameObject toolTip;

    Text toolTipText;

    void Update () {

        if (Input.GetKeyDown(KeyCode.B))
        {
            InventoryScript.instance.OpenClose();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            PlayerStats.instance.TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            SceneManager.LoadSceneAsync(3);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OpenCloseMenu();
        }

        if (Input.GetButtonDown("Inventory"))
        {
            if (!PlayerController.instance.dialogue)
            {
                if (!PlayerController.instance.facingRight)
                {
                    PlayerController.instance.FlipPlayer();
                }

                GameDetails._instance.paused = GameDetails._instance.paused == true ? false : true;
                inventoryCam.SetActive(!inventoryCam.activeSelf);
                equipmentWindow.SetActive(!equipmentWindow.activeSelf);
            }
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
        GameDetails._instance.paused = GameDetails._instance.paused == true ? false : true;
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

        if (code.ToString().Contains("Alpha"))
        {
            tmp.text = code.ToString().Replace("Alpha", string.Empty);
            return;
        }

        // update said text with the correct text
        tmp.text = code.ToString();
    }

    public void ClickActionButton(string buttonName)
    {
        Array.Find(actionButtons, x => x.gameObject.name == buttonName).MyButton.onClick.Invoke();
    }

    /// <summary>
    /// Handles showing the tooltip whenever a mouse is 
    /// hovered over an item that requires a tooltip
    /// </summary>
    public void ShowToolTip(Vector3 pos, IDescribable description, float size = 1)
    {
        toolTip.SetActive(true);
        toolTip.transform.position = pos;
        toolTipText.text = description.GetDescription();
        toolTip.GetComponent<RectTransform>().localScale = new Vector3(size, size, size);
    }

    /// <summary>
    /// Handles hiding the tooltip whenever the mouse
    /// is no longer over an element that requires it
    /// </summary>
    public void HideToolTip()
    {
        toolTip.SetActive(false);
    }


}
