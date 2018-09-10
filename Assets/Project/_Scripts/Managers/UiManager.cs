using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour {

    public static UiManager instance;

    public delegate void KeyLooted(Key key);
    public KeyLooted keyLooted;

    [SerializeField] GameObject box;

    public GameObject inventoryCam;

    [SerializeField] List<Key> keys = new List<Key>();

    [SerializeField] Transform toolTipLocation;

    [SerializeField] GameObject equipmentWindow;

    [SerializeField] ActionButton[] actionButtons;

    [SerializeField] CanvasGroup keybindMenu;

    [SerializeField] GameObject[] keybindButtons;

    [SerializeField] Canvas mainMenu;

    GameDetails gameDetails;

    [SerializeField] Camera maskCamera;

    /// <summary>
    /// Needed by the playercontroller to stop him firing uncontrollably even when tooltip is up
    /// </summary>
    public GameObject toolTip;

    Text toolTipTitle, toolTipStats;

    Key currentKey;

    /// <summary>
    /// Currently selected Key
    /// </summary>
    public Key MyCurrentKey
    {
        get
        {
            return currentKey;
        }

        set
        {
            currentKey = value;
        }
    }

    public List<Key> MyKeys
    {
        get
        {
            return keys;
        }

        set
        {
            keys = value;
        }
    }

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

    void Start()
    {
        gameDetails = GameDetails.instance;
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.B))
        {
            InventoryScript.instance.OpenClose();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            mainMenu.gameObject.SetActive(!mainMenu.gameObject.activeSelf);
            gameDetails.paused = mainMenu.gameObject.activeSelf;
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
                inventoryCam.SetActive(!inventoryCam.activeSelf);
                equipmentWindow.SetActive(!equipmentWindow.activeSelf);

                maskCamera.gameObject.SetActive(!inventoryCam.activeSelf);
                HideToolTip();
            }
        }

        ///Debugging commands go here
        if (DebugControl.debugOn)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                PlayerStats.instance.TakeDamage(10);

                Vector3 pos = PlayerController.instance.transform.position;

                GameObject tmp = ParticleSystemHolder.instance.PlaySpellEffect(pos, "level up");
                tmp.transform.parent = PlayerController.instance.avoidFlip;
                SoundManager.instance.PlayUiSound("levelup");

                var text = CombatTextManager.instance.FetchText(pos);
                var textScript = text.GetComponent<CombatText>();
                textScript.White("Level up!", pos);
                text.transform.position = pos;
                text.SetActive(true);
                textScript.FadeOut();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                Instantiate(box, new Vector3(PlayerController.instance.transform.position.x, PlayerController.instance.transform.position.y, -0.25f), Quaternion.identity);
                Screen.fullScreen = true;
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                Vector2 tmp = PlayerController.instance.transform.position;
                Debug.Log("My Current Position is " + tmp);
                StartCoroutine(PlayerController.instance.SpawnIn());
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                //SceneManager.LoadSceneAsync(3);
                int r = UnityEngine.Random.Range(0, 2);
                EquipmentGenerator._instance.CreateDroppable(PlayerController.instance.transform.position, r);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                OpenCloseMenu();
            }


            if (Input.GetKeyDown(KeyCode.J))
            {
                Bag bag = (Bag)Instantiate(InventoryScript.instance.itemListForDebugging[0]);
                bag.Initialize(16);
                bag.Use();
            }


            if (Input.GetKeyDown(KeyCode.T))
            {
                Bag bag = (Bag)Instantiate(InventoryScript.instance.itemListForDebugging[0]);
                bag.Initialize(16);
                InventoryScript.instance.AddItem(bag);
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                Item potion = Instantiate(InventoryScript.instance.itemListForDebugging[1]);
                InventoryScript.instance.AddItem(potion);
            }

            /// Spawn a new dungeon
            if (Input.GetKeyDown(KeyCode.M))
            {
               // UsefulShortcuts.ClearConsole();
                StartCoroutine(GameDetails.MyInstance.FadeOutAndLoadScene("Caves_dungeon_indoor", "Testing Loading"));
            }

            /// Remove Boss room key
            if (Input.GetKeyDown(KeyCode.N))
            {

                Item tmp = InventoryScript.instance.FindItemInInventory("Boss Room Key");
                tmp.Remove();
            }

            /// Add Gold
            if (Input.GetKeyDown(KeyCode.G))
            {
                CurrencyManager.wealth += 100;
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

    public void RefreshToolTip(IDescribable description, bool showSaleValue = true, float size = 1)
    {
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
