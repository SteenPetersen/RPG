using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour {

    public static UiManager instance;

    [SerializeField] GameObject box;

    public GameObject inventoryCam;

    [SerializeField] Transform toolTipLocation;

    [SerializeField] private GameObject equipmentWindow;

    [SerializeField] private ActionButton[] actionButtons;

    [SerializeField] private CanvasGroup keybindMenu;

    [SerializeField] private GameObject[] keybindButtons;

    [SerializeField] GameObject toolTip;

    Text toolTipTitle, toolTipStats;

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

        toolTipTitle = toolTip.transform.Find("Item_Title").GetComponent<Text>();
        toolTipStats = toolTip.transform.Find("Item_Stats").GetComponent<Text>();
        keybindButtons = GameObject.FindGameObjectsWithTag("Keybind");
    }

    void Update () {

        if (Input.GetKeyDown(KeyCode.B))
        {
            InventoryScript.instance.OpenClose();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            PlayerStats.instance.TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Instantiate(box, new Vector3(PlayerController.instance.transform.position.x, PlayerController.instance.transform.position.y, -0.25f), Quaternion.identity);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            //SceneManager.LoadSceneAsync(3);
            EquipmentGenerator._instance.CreateDroppable(PlayerController.instance.transform.position);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OpenCloseMenu();
        }

        if (Input.GetButtonDown("Inventory"))
        {
            Debug.Log("stopping player walking");


            if (!PlayerController.instance.dialogue)
            {
                if (!PlayerController.instance.facingRight)
                {
                    PlayerController.instance.FlipPlayer();
                }

                PlayerController.instance.anim.Rebind();
                PlayerController.instance.anim.SetTrigger("ForceStopWalk");
                GameDetails._instance.paused = GameDetails._instance.paused == true ? false : true;
                inventoryCam.SetActive(!inventoryCam.activeSelf);
                equipmentWindow.SetActive(!equipmentWindow.activeSelf);
                HideToolTip();
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
    public void ShowToolTip(Vector3 pos, IDescribable description, bool showSaleValue = true, float size = 1)
    {
        toolTip.SetActive(true);
        toolTip.transform.position = pos;
        toolTipTitle.text = description.GetTitle();
        toolTipStats.text = description.GetDescription(showSaleValue);
        toolTip.GetComponent<RectTransform>().localScale = new Vector3(size, size, size);
    }


    /// <summary>
    /// Handles showing the tooltip when the equipment window is open
    /// </summary>
    public void ShowToolTipEquipmentView(IDescribable description, bool showSaleValue = true, float size = 1)
    {
        toolTip.SetActive(true);
        toolTip.transform.position = toolTipLocation.position;
        toolTipTitle.text = description.GetTitle();
        toolTipStats.text = description.GetDescription(showSaleValue);
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
