﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDetails : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void SyncFiles();

    [DllImport("__Internal")]
    private static extern void WindowAlert(string message);

    Canvas[] ui;


    #region Singleton
    public static GameDetails _instance;

    public static GameDetails Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
        } 

        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    #region Dialogue
    public Vector3 dialogueNPCIsStandingOnTheRight = new Vector3(-1.5f, -9f, -6.3f);
    public Vector3 dialogueNPCIsStandingOnTheLeft = new Vector3(1.86f, -9f, -6.3f);

    public GameObject dialogueCamera;

    #endregion


    // statics
    public static int dungeonLevel;

    // used for deathScreen statistics
    public static int enemiesKilled;
    public static int dungeonFloorsExplored;
    public static int ripostes;
    public static int blocks;
    public static int hits;
    public static int fullChargeHits;
    public static int arrowsFired;



    /// <summary>
    /// Accessed by DungeonLevelLoadLogic in order to fade the screen when loading another level
    /// </summary>
    public float fadeSpeed;
    public Image fadeToBlack;

    public ParticleSystem gameSaved;

    public int stage;
    public int kingSpeech;
    public bool paused, loadingScene;
    public GameObject[] generalObjects;

    public Plane m_Plane = new Plane(Vector3.forward, Vector3.zero);

    public ActionButton actionbar1;
    public ActionButton actionbar2;
    public ActionButton actionbar3;


    public GameObject player;
    PlayerStats playerStats;
    float normalSpeed = 0.8f;

    // Tick event timer
    [SerializeField] float tickTime, tickTimer;

    private void OnEnable()
    {
        // subscribe to notice is a scene is loaded.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // if player does not have a saved game
        if (!File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            // then create one
            Save();
        }

        if (player == null)
        {
            player = PlayerController.instance.gameObject;
        }

        playerStats = player.GetComponent<PlayerStats>();

        ui = gameObject.transform.GetComponentsInChildren<Canvas>();

        // references to action bars

        //StartCoroutine(enableUi());
    }

    void Update()
    {
        if (paused)
        {
            Time.timeScale = 0;
        }
        else if (!paused)
        {
            Time.timeScale = 1;
        }

        // start death sequence
        if (PlayerController.instance.isDead && fadeToBlack.color.a <= 1)
        {
            DeathSequence();
        }

        if (!PlayerController.instance.isDead)
        {
            tickTimer += Time.deltaTime;

            if (tickTimer > tickTime)
            {
                tickTimer = 0;
                TickEvent();
            }
        }

    }

    private void TickEvent()
    {
        //Debug.Log("tick event");

        if (PlayerStats.instance.currentHealth < PlayerStats.instance.maxHealth ||
            PlayerStats.instance.MyCurrentStamina < PlayerStats.instance.MyMaxStamina)
        {
            PlayerStats.instance.Regen();
        }
    }

    private void DeathSequence()
    {
        fadeToBlack.enabled = true;
        fadeToBlack.color = new Color(0, 0, 0, fadeSpeed);
        fadeSpeed += 0.01f;


        if (fadeToBlack.color.a >= 1 && !loadingScene)
        {
            PlayerController.instance.anim.SetLayerWeight(1, 0);

            SceneManager.LoadScene("DeathScreen");

            // disallow player to animate walking

            //if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
            //{
            //    Load();
            //}
            //else
            //{
            //    PlayerController.instance.gameObject.transform.position = new Vector2(-12f, 4);
            //    playerStats.Heal((int)playerStats.maxHealth);
            //    SceneManager.LoadScene(0);
            //    PlayerController.instance.isDead = false;
            //    loadingScene = true;
            //}
        }
    }

    public void KillPlayer()
    {
        Debug.Log("killing player");
        PlayerController.instance.enemies.Clear();
    }

    public void Save()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene == 1)
            return;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

        PlayerData data = new PlayerData();

        // save Stats
        data.currentHealth = playerStats.currentHealth;
        data.maxHealth = playerStats.maxHealth;
        data.experience = ExperienceManager.instance.experience;
        data.level = ExperienceManager.instance.level;
        data.experienceNeeded = ExperienceManager.instance.experienceRequired;
        data.Stamina = playerStats.Sta.GetBaseValue();
        data.Strength = playerStats.Str.GetBaseValue();
        data.Agility = playerStats.Agi.GetBaseValue();
        data.Damage = playerStats.damage.GetBaseValue();
        data.Armor = playerStats.armor.GetBaseValue();


        data.questLine = StoryManager.questLine;
        data.stage = StoryManager.stage;
        data.givenItems = StoryManager.givenItems;
        data.tutorialConversation = StoryManager.tutorialConversation;


        // bags
        data.amountOfBags = AmountOfBags();
        data.amountOfSlotsPerBag = GetAmountOfSlotsPerBag();

        //equipped Items
        data.equippedItems = GetEquippedItemsToSave();

        // Items in bags
        data.amountOfStackableItemsPerSlot = AmountOfStackableItemsPerSlot();
        data.itemsInTheSlot = ItemsInTheSlot();

        // currency
        data.gold = CurrencyManager.wealth;

        // action bars
        //Debug.Log(actionbar2.MyUseable.ToString());

        if (actionbar1.SaveItemData != string.Empty)
        {
            data.actionbar1 = actionbar1.SaveItemData;
        }
        if (actionbar2.SaveItemData != string.Empty)
        {
            data.actionbar2 = actionbar2.SaveItemData;
        }
        if (actionbar3.SaveItemData != string.Empty)
        {
            data.actionbar3 = actionbar3.SaveItemData;
        }

        data.zone = SceneManager.GetActiveScene().buildIndex;
        data.locationX = player.transform.position.x;
        data.locationY = player.transform.position.y;


        bf.Serialize(file, data);
        file.Close();
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            SyncFiles();
        }

        gameSaved.Play();
        Debug.Log("saved");
    }

    public void Load()
    {
        Debug.Log("load land");
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            Debug.Log("found file");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            DestroyAllPlayerPossessionsInBags();
            DestroyAllCurrentlyEquippedGear();
            DestroyAllNonDefaultBags();
            ExperienceManager.instance.experience = 0;


            // Load Stats
            PlayerController.instance.speed = normalSpeed;
            playerStats.currentHealth = 0;
            playerStats.maxHealth = data.maxHealth;
            playerStats.Heal((int)data.currentHealth);
            ExperienceManager.instance.level = data.level;
            ExperienceManager.instance.experienceRequired = data.experienceNeeded;
            playerStats.Agi.SetValue(data.Agility);
            playerStats.Sta.SetValue(data.Stamina);
            playerStats.Str.SetValue(data.Strength);
            playerStats.damage.SetValue(data.Damage);
            playerStats.armor.SetValue(data.Armor);

            playerStats.UpdateStats();

            ExperienceManager.instance.AddExpFromLoadedGame(data.experience);

            StoryManager.questLine = data.questLine;
            StoryManager.stage = data.stage;
            StoryManager.givenItems = data.givenItems;
            StoryManager.tutorialConversation = data.tutorialConversation;

            // bags
            LoadBags(data.amountOfBags, data.amountOfSlotsPerBag);

            // currently equipped items
            LoadEquippedItems(data.equippedItems);

            // items in bags
            LoadAllItemsInBags(data.itemsInTheSlot, data.amountOfStackableItemsPerSlot);

            // currency
            CurrencyManager.wealth = data.gold;

            // action bars
            Item tmp = InventoryScript.instance.FindItemInInventory(data.actionbar1);
            if (tmp != null)
            {
                actionbar1.LoadGameUseable(tmp as IUseable, tmp);
            }
            tmp = InventoryScript.instance.FindItemInInventory(data.actionbar2);
            if (tmp != null)
            {
                actionbar2.LoadGameUseable(tmp as IUseable, tmp);
            }
            tmp = InventoryScript.instance.FindItemInInventory(data.actionbar3);
            if (tmp != null)
            {
                actionbar3.LoadGameUseable(tmp as IUseable, tmp);
            }

            SceneManager.LoadScene(data.zone);
            player.transform.position = new Vector2(data.locationX, data.locationY);


        }
    }

    #region Destroy
    private void DestroyAllCurrentlyEquippedGear()
    {
        List<Equipment> items = new List<Equipment>();

        for (int i = 0; i < EquipmentManager.instance.currentEquipment.Count; i++)
        {
            if (EquipmentManager.instance.currentEquipment[i] == null)
            {
                items.Add(EquipmentManager.instance.currentEquipment[i]);
            }

            if (EquipmentManager.instance.currentEquipment[i] != null)
            {
                items.Add(EquipmentManager.instance.currentEquipment[i]);
            }
        }


        foreach (var item in items)
        {
            EquipmentManager.instance.UnequipAll();
        }

        DestroyAllPlayerPossessionsInBags();

    }

    private void DestroyAllPlayerPossessionsInBags()
    {
        foreach (SlotScript slot in InventoryScript.instance.GetAllSlots())
        {
            slot.MyItems.Clear();
            slot.RemoveItem(slot.MyItem);
        }

        //for (int i = InventoryScript.instance.MyBags.Count - 1; i >= 0; i--)
        //{
        //    Destroy(InventoryScript.instance.MyBags[i]);
        //}



    }

    /// <summary>
    /// Destroys all bags except the one the player starts with, then destroys all the items that were inside the bags
    /// </summary>
    private void DestroyAllNonDefaultBags()
    {
        for (int i = InventoryScript.instance.MyBagButtons.Length - 1; i >= 1; i--)
        {
            BagButton bagButton = InventoryScript.instance.MyBagButtons[i];

            if (bagButton.MyBag != null)
            {
                bagButton.RemoveBag();
            }
        }

        DestroyAllPlayerPossessionsInBags();
    }

    #endregion Destroy

    #region Bags
    int AmountOfBags()
    {
        return InventoryScript.instance.MyBags.Count - 1;
    }

    List<int> GetAmountOfSlotsPerBag()
    {
        List<int> slotsPerBag = new List<int>();

        foreach (Bag bag in InventoryScript.instance.MyBags)
        {
            slotsPerBag.Add(bag.Slots);
        }

        return slotsPerBag;
    }

    void LoadBags(int bags, List<int> slotsPerBag)
    {
        for (int i = 0; i < bags; i++)
        {
            Bag bag = (Bag)Instantiate(InventoryScript.instance.MyItems[0]);
            bag.Initialize(slotsPerBag[i + 1]);
            InventoryScript.instance.AddItem(bag);
        }
    }

    #endregion Bags

    #region Equipped items
    List<string> GetEquippedItemsToSave()
    {
        List<string> items = new List<string>();

        for (int i = 0; i < EquipmentManager.instance.currentEquipment.Count; i++)
        {
            if (EquipmentManager.instance.currentEquipment[i] == null)
            {
                items.Add(string.Empty);
            }

            if (EquipmentManager.instance.currentEquipment[i] != null)
            {
                items.Add(EquipmentManager.instance.currentEquipment[i].name);
            }
        }
        
        return items;
    }

    void LoadEquippedItems(List<string> items)
    {
        List<Equipment> tmp = new List<Equipment>(EquipmentManager.instance.currentEquipment.Count);

        foreach (var item in items)
        {
            if (item == string.Empty)
            {
                tmp.Add(null);
            }
            else
            {
                var tmpEquip = Instantiate(Resources.Load("Equipment/" + item, typeof(Equipment))) as Equipment;
                tmp.Add(tmpEquip);
            }
        }

        for (int i = 0; i < EquipmentManager.instance.currentEquipment.Count; i++)
        {
            if (tmp[i] != null)
            {
                EquipmentManager.instance.Equip(tmp[i]);
            }
        }
    }

    #endregion Equipped items

    #region Items In Bags

    List<string> ItemsInTheSlot()
    {
        List<string> itemInSlot = new List<string>();

        foreach (SlotScript slot in InventoryScript.instance.GetAllSlots())
        {
            if (slot.IsEmpty)
            {
                itemInSlot.Add(string.Empty);
            }
            else if (slot.MyCount > 0)
            {
                itemInSlot.Add(slot.MyItems.Peek().name);
            }

        }

        return itemInSlot;
    }

    List<int> AmountOfStackableItemsPerSlot()
    {
        List<int> itemsPerSlot = new List<int>();

        foreach (SlotScript slot in InventoryScript.instance.GetAllSlots())
        {
            itemsPerSlot.Add(slot.MyCount);
        }

        return itemsPerSlot;
    }

    void LoadAllItemsInBags(List<string> itemInSlot, List<int> amountOfitemsPerSlot)
    {
        int count = 0;

        foreach (SlotScript slot in InventoryScript.instance.GetAllSlots())
        {
            //Debug.Log("running through slot " + count + " there are: " + amountOfitemsPerSlot[count] + "items in this slot. And the item is :" + itemInSlot[count]);
            if (amountOfitemsPerSlot[count] == 0)
            {
                count++;
                continue;
            }
            else if (amountOfitemsPerSlot[count] == 1)
            {
                //Debug.Log("inside 1 slot function");
                if (Resources.Load("Equipment/" + itemInSlot[count]) != null)
                {
                    //Debug.Log("inside resource test");
                    var tmpEquip = Instantiate(Resources.Load("Equipment/" + itemInSlot[count], typeof(Equipment))) as Equipment;
                    slot.AddItem(tmpEquip);
                }
                else if (Resources.Load("Equipment/" + itemInSlot[count]) == null)
                {
                    var tmpItem = Instantiate(Resources.Load("Items/" + itemInSlot[count], typeof(Item))) as Item;
                    slot.AddItem(tmpItem);
                }
                count++;
            }
            else if (amountOfitemsPerSlot[count] > 1)
            {
                for (int i = 0; i < amountOfitemsPerSlot[count]; i++)
                {
                    var tmpItem = Instantiate(Resources.Load("Items/" + itemInSlot[count], typeof(Item))) as Item;
                    slot.AddItem(tmpItem);
                }
                count++;
            }
            UiManager.instance.UpdateStackSize(slot);
        }
        
    }

    #endregion Items In Bags

    private void PlatformSafeMessage(string message)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            WindowAlert(message);
        }
        else
        {
            Debug.Log(message);
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            File.Delete(Application.persistentDataPath + "/playerInfo.dat");
        }
    }

    private void OnDisable()
    {
        // unsubscribe from the scenemanager.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // if you dont know who the player is anymore
        if (player == null)
        {
            // find the player
            player = GameObject.Find("Player");
        }

        // if you dont know what the playerStats are anymore
        if (playerStats == null)
        {
            // get them from the player
            playerStats = player.GetComponent<PlayerStats>();
        }

        // if the player is loading in dead
        if (PlayerController.instance.isDead && scene.name != "DeathScreen")
        {
            // undeadify the player
            PlayerController.instance.isDead = false;

            // reallow player to animate walking
            PlayerController.instance.anim.SetLayerWeight(1, 1);

            // allow player to animate back to life - get it?
            PlayerController.instance.anim.SetTrigger("LoadGame");
        }

        // If you know what the Ui is
        if (ui != null)
        {
            // iterate through all the elements in the UI 
            foreach (Canvas canvas in ui)
            {
                // and enable them
                canvas.enabled = true;

            }
        }

        // in case there are enemies nearby when a Load happens clear all enemies from the list
        PlayerController.instance.enemies.Clear();

        var m_Scene = SceneManager.GetActiveScene();

        if (m_Scene.name != "Loading")
        {
            StartCoroutine(enableUi());
            Debug.Log("being called inside OnSceneLoaded");
        }


    }

    IEnumerator enableUi()
    {
        yield return new WaitForSeconds(1.5f);

        StartCoroutine(UnFade());
        loadingScene = false;
        Debug.Log("being called inside enableUI");
    }

    IEnumerator UnFade()
    {
        while (fadeToBlack.color.a >= 0.02)
        {
            fadeToBlack.enabled = true;
            fadeToBlack.color = new Color(0, 0, 0, fadeSpeed);
            fadeSpeed -= 0.02f;
            yield return new WaitForSeconds(0.01f);
        }

        if (AstarPath.active != null)
        {
            var m_Scene = SceneManager.GetActiveScene();

            if (m_Scene.name.Contains("_indoor"))
            {
                Debug.Log("calling boardcreator method");
                BoardCreator.instance.CreateDungeonGraph();
                StopCoroutine("UnFade");
                Debug.Log("Ending Coroutine on GameDetails, board should now be created and have 2D grid.");
                DrawDistanceActivator.instance.StartCoroutine("Check");
            }

            //AstarPath.active.Scan();
        }
    }
}

[Serializable]
class PlayerData
{
    // Stats
    public float currentHealth;
    public float maxHealth;
    public float experience;
    public float experienceNeeded;
    public int level;
    public int Stamina;
    public int Agility;
    public int Strength;
    public int Damage;
    public int Armor;


    // Progress
    public int stage;
    public int questLine;
    public int givenItems;
    public int tutorialConversation;


    // currently equipped bags
    public int amountOfBags;
    public List<int> amountOfSlotsPerBag;

    // Currently equipped Items
    public List<string> equippedItems;

    // Items in Bag
    public List<int> amountOfStackableItemsPerSlot;
    public List<string> itemsInTheSlot;

    // currency
    public int gold;

    // Action Bars
    public string actionbar1;
    public string actionbar2;
    public string actionbar3;

    // zone information
    public int zone;
    public float locationX;
    public float locationY;

}
