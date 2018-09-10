using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameDetails : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void SyncFiles();

    [DllImport("__Internal")]
    private static extern void WindowAlert(string message);

    Canvas[] ui;


    #region Singleton
    public static GameDetails instance;

    public static GameDetails MyInstance { get { return instance; } }

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

        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    #region Dialogue
    public Vector3 dialogueNPCIsStandingOnTheRight = new Vector3(-1.5f, -9f, -6.3f);
    public Vector3 dialogueNPCIsStandingOnTheLeft = new Vector3(1.86f, -9f, -6.3f);

    public GameObject dialogueCamera;

    #endregion

    /// <summary>
    /// Player entered Information
    /// </summary>
    public static string playerName = "Pixl";

    /// <summary>
    /// Static variables used for deathScreen statistics
    /// </summary>
    public static int enemiesKilled;
    public static int dungeonFloorsExplored;
    public static int ripostes;
    public static int blocks;
    public static int hits;
    public static int fullChargeHits;
    public static int arrowsFired;
    public static int randomizedItemsDropped;

    /// <summary>
    /// Public bools for turning things on or off
    /// </summary>
    public static bool soundEffects = true;
    public static bool music = true;

    /// <summary>
    /// Accessed by DungeonLevelLoadLogic in order to fade the screen when loading another level
    /// </summary>
    public float fadeSpeed;
    public CanvasGroup fadeToBlack;
    [SerializeField] TextMeshProUGUI loadScreenText;
    [SerializeField] TextMeshProUGUI loadScreenTip;
    [SerializeField] string[] tips;
    [SerializeField] RawImage particleImage;

    public ParticleSystem gameSaved;

    public int stage;
    public int kingSpeech;
    public bool paused, loadingScene, slowMotion;
    public GameObject[] generalObjects;

    public Plane m_Plane = new Plane(Vector3.forward, Vector3.zero);

    public ActionButton actionbar1;
    public ActionButton actionbar2;
    public ActionButton actionbar3;


    public GameObject player;
    PlayerStats playerStats;
    float normalSpeed = 1.6f;

    // Tick event timer
    [SerializeField] float tickTime, tickTimer;
    bool deathSequenceInitiated;

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
            Save(SceneManager.GetActiveScene(), true);
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

        if (slowMotion)
        {
            Time.timeScale = 0.2f;
        }

        else if (!paused)
        {
            Time.timeScale = 1;
        }

        // start death sequence
        if (PlayerController.instance.isDead && !deathSequenceInitiated)
        {
            StartCoroutine(DeathSequence());
            deathSequenceInitiated = true;
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

        if (PlayerStats.instance.MyCurrentHealth < PlayerStats.instance.MyMaxHealth ||
            PlayerStats.instance.MyCurrentStamina < PlayerStats.instance.MyMaxStamina)
        {
            PlayerStats.instance.Regen();
        }
    }

    IEnumerator DeathSequence()
    {
        if (GameObject.Find("FollowLight") != null)
        {
            GameObject.Find("FollowLight").SetActive(false);
        }

        PlayerController.instance.anim.SetLayerWeight(1, 0);

        while (fadeToBlack.alpha <= 0.98)
        {
            fadeToBlack.gameObject.SetActive(true);
            fadeToBlack.alpha += 0.02f;
            yield return new WaitForSeconds(0.01f);
        }

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

    public void KillPlayer()
    {
        Debug.Log("killing player");
        PlayerController.instance.enemies.Clear();
    }

    public void Save(bool safeSpot = false)
    {
        Scene currentScene = SceneManager.GetActiveScene();

        Save(currentScene, safeSpot);
    }

    public void Save(Scene scene, bool safeSpot = false)
    {
        if (scene.name.EndsWith("_indoor"))
            return;

        Debug.Log("Entered scene dependant saving");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

        PlayerData data = new PlayerData();

        // save Stats
        data.currentHealth = playerStats.MyCurrentHealth;
        data.maxHealth = playerStats.MyMaxHealth;
        data.experience = ExperienceManager.instance.experience;
        data.level = ExperienceManager.MyLevel;
        data.experienceNeeded = ExperienceManager.instance.experienceRequired;
        data.Stamina = playerStats.Sta.GetBaseValue();
        data.Strength = playerStats.Str.GetBaseValue();
        data.Agility = playerStats.Agi.GetBaseValue();
        data.Damage = playerStats.damage.GetBaseValue();
        data.Armor = playerStats.armor.GetBaseValue();

        // Talents
        data.projectile = PlayerTalents.instance.MyProjectile;

        data.questLine = StoryManager.questLine;
        data.stage = StoryManager.stage;
        data.givenItems = StoryManager.givenItems;
        data.tutorialConversation = StoryManager.tutorialStage;


        // bags
        data.amountOfBags = AmountOfBags();
        data.amountOfSlotsPerBag = GetAmountOfSlotsPerBag();

        //equipped Items
        data.equippedItems = GetEquippedItemsToSave();

        // Items in bags
        data.amountOfStackableItemsPerSlot = AmountOfStackableItemsPerSlot();
        data.itemsInTheSlot = GetItemsInBagToSave();

        // currency
        data.gold = CurrencyManager.wealth;

        // action bars
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

        if (scene.name.Contains("Death"))
        {
            ResetStaticData();

            /// Places player in the first zone at its homePosition
            string startScene = "Start_Area";
            data.zone = startScene;

            Debug.Log(data.zone + "!" + startScene);

            Vector2 vec = SceneControl.instance.MyCurrentHomePosition(startScene);
            data.locationX = vec.x;
            data.locationY = vec.y;

            Debug.Log("Death Screen save Game paramaters used");
        }
        else
        {
            data.zone = scene.name;

            if (!safeSpot)
            {
                data.locationX = player.transform.position.x;
                data.locationY = player.transform.position.y;
            }
            else if (safeSpot)
            {
                Vector2 vec = SceneControl.instance.MyCurrentHomePosition(SceneManager.GetActiveScene().name);
                data.locationX = vec.x;
                data.locationY = vec.y;
            }
        }

        // Statics
        data.dungeonFloorsExplored = dungeonFloorsExplored;
        data.enemiesKilled = enemiesKilled;
        data.ripostes = ripostes;
        data.blocks = blocks;
        data.hits = hits;
        data.fullChargeHits = fullChargeHits;
        data.arrowsFired = arrowsFired;
        data.randomizedItemsDropped = randomizedItemsDropped;

        bf.Serialize(file, data);
        file.Close();

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            SyncFiles();
        }

        gameSaved.Play();

    }

    public void Load(bool wasDead = false)
    {
        //Debug.Log("load land");
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            //Debug.Log("found file");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            DestroyAllPlayerPossessionsInBags();
            DestroyAllCurrentlyEquippedGear();
            DestroyAllNonDefaultBags();
            ExperienceManager.instance.experience = 0;

            //Talents
            PlayerTalents.instance.MyProjectile = data.projectile;

            // Load Stats
            PlayerController.instance.speed = normalSpeed;
            playerStats.MyCurrentHealth = 0;
            playerStats.MyMaxHealth = data.maxHealth;
            playerStats.Heal((int)data.currentHealth);
            ExperienceManager.MyLevel = data.level;
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
            StoryManager.tutorialStage = data.tutorialConversation;

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

            // statics
            dungeonFloorsExplored = data.dungeonFloorsExplored;
            enemiesKilled = data.enemiesKilled;
            ripostes = data.ripostes;
            blocks = data.blocks;
            hits = data.hits;
            fullChargeHits = data.fullChargeHits;
            arrowsFired = data.arrowsFired;
            randomizedItemsDropped = data.randomizedItemsDropped;

            SceneManager.LoadScene(data.zone.ToString());
            player.transform.position = new Vector2(data.locationX, data.locationY);

            if (wasDead)
            {
                ResetStaticData();
            }

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
    List<ItemData> GetEquippedItemsToSave()
    {
        List<ItemData> items = new List<ItemData>();

        for (int i = 0; i < EquipmentManager.instance.currentEquipment.Count; i++)
        {
            if (EquipmentManager.instance.currentEquipment[i] == null)
            {
                items.Add(null);
            }

            if (EquipmentManager.instance.currentEquipment[i] != null)
            {
                if (Resources.Load("PremadeItems/" + EquipmentManager.instance.currentEquipment[i].name) != null)
                {
                    //Equipment t = Instantiate(Resources.Load("Equipment/" + item.name, typeof(Equipment))) as Equipment;
                    //tmp.Add(t);

                    ItemData defaultItem = new ItemData();
                    defaultItem.name = EquipmentManager.instance.currentEquipment[i].name;
                    Debug.Log("Saving a default item " + defaultItem.name);
                    items.Add(defaultItem);
                    continue;
                }

                Equipment current = EquipmentManager.instance.currentEquipment[i];
                ItemData tmp = new ItemData();

                tmp.tier = current.tier;
                tmp.name = current.name;
                tmp.title = current.GetTitle();
                tmp.graphicId = current.graphicId;

                tmp.type = current.equipType;
                tmp.slot = current.equipSlot;
                tmp.quality = current.MyQuality;
                tmp.armorType = current.armorType;

                tmp.dmgMod = current.damageModifier;
                tmp.armorMod = current.armorModifier;
                tmp.sta = current.sta;
                tmp.str = current.str;
                tmp.agi = current.agi;
                tmp.rangedProjectile = current.rangedProjectile;
                tmp.sellValue = current.sellValue;
                tmp.buyValue = current.buyValue;

                items.Add(tmp);
            }
        }
        
        return items;
    }

    List<ItemData> GetItemsInBagToSave()
    {
        List<ItemData> itemInSlot = new List<ItemData>();

        foreach (SlotScript slot in InventoryScript.instance.GetAllSlots())
        {
            if (slot.IsEmpty)
            {
                itemInSlot.Add(null);
            }
            else if (slot.MyCount > 0)
            {
                Item current = slot.MyItems.Peek();
                ItemData tmp = new ItemData();

                if (current is Equipment)
                {
                    if (Resources.Load("PremadeItems/" + current.name) != null)
                    {
                        //Equipment t = Instantiate(Resources.Load("Equipment/" + item.name, typeof(Equipment))) as Equipment;
                        //tmp.Add(t);

                        ItemData defaultItem = new ItemData();
                        defaultItem.name = current.name;
                        Debug.Log("Saving a default item " + defaultItem.name);
                        itemInSlot.Add(defaultItem);
                        continue;
                    }

                    Equipment equip = slot.MyItems.Peek() as Equipment;
                    tmp.tier = equip.tier;
                    tmp.name = equip.name;
                    tmp.title = equip.GetTitle();
                    tmp.graphicId = equip.graphicId;

                    tmp.type = equip.equipType;
                    tmp.slot = equip.equipSlot;
                    tmp.quality = equip.MyQuality;
                    tmp.armorType = equip.armorType;

                    tmp.dmgMod = equip.damageModifier;
                    tmp.armorMod = equip.armorModifier;
                    tmp.sta = equip.sta;
                    tmp.str = equip.str;
                    tmp.agi = equip.agi;
                    tmp.rangedProjectile = equip.rangedProjectile;
                    tmp.sellValue = equip.sellValue;
                    tmp.buyValue = equip.buyValue;

                }

                else
                {
                    tmp.name = current.name;
                    tmp.title = current.GetTitle();
                }

                itemInSlot.Add(tmp);

            }

        }

        return itemInSlot;
    }

    void LoadEquippedItems(List<ItemData> items)
    {
        List<Equipment> tmp = new List<Equipment>(EquipmentManager.instance.currentEquipment.Count);

        foreach (var item in items)
        {
            if (item == null)
            {
                tmp.Add(null);
            }
            else
            {
                if (Resources.Load("PremadeItems/" + item.name) != null)
                {
                    Equipment t = Instantiate(Resources.Load("PremadeItems/" + item.name, typeof(Equipment))) as Equipment;
                    tmp.Add(t);
                }
                else if (Resources.Load("PremadeItems/" + item.name) == null)
                {
                    Equipment tmpEquip = EquipmentGenerator._instance.CreateLoadedItem(item);
                    tmp.Add(tmpEquip);
                }

            }
        }

        for (int i = 0; i < EquipmentManager.instance.currentEquipment.Count; i++)
        {
            if (tmp[i] != null)
            {
                EquipmentManager.instance.Equip(tmp[i], true);
            }
        }
    }

    void LoadAllItemsInBags(List<ItemData> itemInSlot, List<int> amountOfitemsPerSlot)
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
                if (Resources.Load("PremadeItems/" + itemInSlot[count].name) != null)
                {
                    Item tmpEquip = Instantiate(Resources.Load("PremadeItems/" + itemInSlot[count].name, typeof(Item))) as Item;
                    slot.AddItem(tmpEquip);
                }
                else if (Resources.Load("PremadeItems/" + itemInSlot[count].name) == null)
                {
                    var tmpItem = EquipmentGenerator._instance.CreateLoadedItem(itemInSlot[count]);
                    slot.AddItem(tmpItem);
                }
                count++;
            }
            // if there is more than 1 item in the slot then it is not a generated item therefore look 
            // in the resources for the item with the name we saved it under.
            else if (amountOfitemsPerSlot[count] > 1)
            {
                for (int i = 0; i < amountOfitemsPerSlot[count]; i++)
                {
                    var tmpItem = Instantiate(Resources.Load("PremadeItems/" + itemInSlot[count].name, typeof(Item))) as Item;
                    slot.AddItem(tmpItem);
                }
                count++;
            }
            UiManager.instance.UpdateStackSize(slot);
        }

    }


    #endregion Equipped items

    #region Items In Bags



    List<int> AmountOfStackableItemsPerSlot()
    {
        List<int> itemsPerSlot = new List<int>();

        foreach (SlotScript slot in InventoryScript.instance.GetAllSlots())
        {
            itemsPerSlot.Add(slot.MyCount);
        }

        return itemsPerSlot;
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

    public void ResetStaticData()
    {
        dungeonFloorsExplored  = 0;
        enemiesKilled          = 0;
        ripostes               = 0;
        blocks                 = 0;
        hits                   = 0;
        fullChargeHits         = 0;
        arrowsFired            = 0;
        randomizedItemsDropped = 0;
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

            deathSequenceInitiated = false;

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
        }


    }

    IEnumerator enableUi()
    {
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(UnFade());
        loadingScene = false;
        //Debug.Log("being called inside enableUI");
    }

    /// <summary>
    /// Unfades the scene and sets the dungeons Astarpath to 
    /// scan if the Scene it is loading is a dungeon
    /// </summary>
    /// <returns></returns>
    public IEnumerator UnFade()
    {
        loadScreenText.text = "Almost done";

        bool createdDungeonGraph = false;

        var m_Scene = SceneManager.GetActiveScene();

        if (m_Scene.name.Contains("dungeon_indoor"))
        {
            if (BoardCreator.instance != null)
            {
                BoardCreator board = BoardCreator.instance;

                createdDungeonGraph = board.CreateDungeonGraph();

                DrawDistanceActivator.instance.StartCoroutine("Check");

                board.SetDoors();
                board.SpawnSecretWalls();

                if (createdDungeonGraph)
                {
                    Debug.Log("I successfully created a graph and am unfading the dungeon");

                    while (fadeToBlack.alpha >= 0.02)
                    {
                        fadeToBlack.gameObject.SetActive(true);
                        fadeToBlack.alpha -= 0.02f;
                        yield return new WaitForSeconds(0.01f);
                    }

                    loadScreenTip.gameObject.SetActive(false);
                    loadScreenText.gameObject.SetActive(false);
                    particleImage.gameObject.SetActive(false);

                    StopCoroutine("UnFade");
                }

                DungeonManager.instance.dungeonReady = true;
            }



            else
            {
                createdDungeonGraph = BoardCreator.instance.CreateDungeonGraph();

                yield return new WaitForSeconds(0.5f);
            }
        }

        else
        {
            while (fadeToBlack.alpha >= 0.02)
            {
                fadeToBlack.gameObject.SetActive(true);
                fadeToBlack.alpha -= 0.02f;
                yield return new WaitForSeconds(0.01f);
            }
        }

        loadScreenTip.gameObject.SetActive(false);
        loadScreenText.gameObject.SetActive(false);
        particleImage.gameObject.SetActive(false);
     
    }

    /// <summary>
    /// Fades out the scene when transitioning to a new scene
    /// </summary>
    /// <param name="zoneToLoad">the Index number of the zone that is to be loaded 
    /// (can be found in the build setting pane)</param>
    /// <returns></returns>
    public IEnumerator FadeOutAndLoadScene(string zoneToLoad)
    {
        loadScreenText.text = "Loading";
        loadScreenTip.text = GetRandomTip();

        loadScreenTip.gameObject.SetActive(true);
        loadScreenText.gameObject.SetActive(true);
        particleImage.gameObject.SetActive(true);

        while (fadeToBlack.alpha <= 0.98)
        {
            fadeToBlack.gameObject.SetActive(true);
            fadeToBlack.alpha += 0.02f;
            yield return new WaitForSeconds(0.01f);
        }


        CameraController.instance.SetHomeRotation();
        SceneManager.LoadSceneAsync(zoneToLoad);

        yield return null;
    }

    /// <summary>
    /// Override to also add a text Fades out the scene when transitioning to a new scene
    /// </summary>
    /// <param name="zoneToLoad">the Index number of the zone that is to be loaded 
    /// (can be found in the build setting pane)</param>
    /// <returns></returns>
    public IEnumerator FadeOutAndLoadScene(string zoneToLoad, string loadingText)
    {
        loadScreenText.text = loadingText;
        loadScreenTip.text = GetRandomTip();

        loadScreenTip.gameObject.SetActive(true);
        loadScreenText.gameObject.SetActive(true);
        particleImage.gameObject.SetActive(true);

        while (fadeToBlack.alpha <= 0.98)
        {
            fadeToBlack.gameObject.SetActive(true);
            fadeToBlack.alpha += 0.02f;
            yield return new WaitForSeconds(0.01f);
        }

        CameraController.instance.SetHomeRotation();
        SceneManager.LoadSceneAsync(zoneToLoad);

        yield return null;
    }


    /// <summary>
    /// Overload Coroutine for also placing Player
    /// </summary>
    /// <param name="zoneToLoad"></param>
    /// <param name="playerPos"></param>
    /// <returns></returns>
    public IEnumerator FadeOutAndLoadScene(string zoneToLoad, Vector2 playerPos)
    {
        fadeToBlack.gameObject.SetActive(true);

        while (fadeToBlack.alpha <= 0.98)
        {
            fadeToBlack.alpha += 0.02f;
            yield return new WaitForSeconds(0.01f);
        }

        PlayerController.instance.transform.position = playerPos;

        CameraController.instance.SetHomeRotation();
        SceneManager.LoadSceneAsync(zoneToLoad);

        yield return null;
    }

    /// <summary>
    /// Fades out the scene when transitioning to a new scene
    /// </summary>
    /// <param name="zoneToLoad">the Index number of the zone that is to be loaded 
    /// (can be found in the build setting pane)</param>
    /// <returns></returns>
    public IEnumerator FadeOutAndLoadGame()
    {
        loadScreenText.text = "...Now where were we?";
        loadScreenTip.text = GetRandomTip();

        loadScreenTip.gameObject.SetActive(true);
        loadScreenText.gameObject.SetActive(true);
        particleImage.gameObject.SetActive(true);

        while (fadeToBlack.alpha <= 0.98)
        {
            fadeToBlack.gameObject.SetActive(true);
            fadeToBlack.alpha += 0.02f;
            yield return new WaitForSeconds(0.01f);
        }

        Load();

        yield return null;
    }

    public IEnumerator SetTimeBackToNormal()
    {
        slowMotion = false;

        while (Time.timeScale < 1)
        {
            Time.timeScale += 0.05f;
            yield return new WaitForSeconds(0.02f);
        }

        Time.timeScale = 1;

        yield return null;
    }

    public IEnumerator LerpTime(float _lerpTimeTo, float _timeToTake, DungeonLevelLoadLogic script)
    {
        slowMotion = false;

        float endTime = Time.time + _timeToTake;
        float startTimeScale = Time.timeScale;
        float i = 0f;
        while (Time.time < endTime)
        {
            i += (1 / _timeToTake) * Time.deltaTime;
            Time.timeScale = Mathf.Lerp(startTimeScale, _lerpTimeTo, i);

            yield return null;
        }
        Time.timeScale = _lerpTimeTo;

        script.routineStarted = false;
    }

    /// <summary>
    /// Retursn a random player tip
    /// </summary>
    /// <returns></returns>
    private string GetRandomTip()
    {
        int rndText = UnityEngine.Random.Range(0, tips.Length);

        return tips[rndText];
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

    // Talents
    public int projectile = 0;


    // currently equipped bags
    public int amountOfBags;
    public List<int> amountOfSlotsPerBag;

    // Currently equipped Items
    public List<ItemData> equippedItems;

    // Items in Bag
    public List<int> amountOfStackableItemsPerSlot;
    public List<ItemData> itemsInTheSlot;

    // currency
    public int gold;

    // Action Bars
    public string actionbar1;
    public string actionbar2;
    public string actionbar3;

    // zone information
    public string zone;
    public float locationX;
    public float locationY;

    // Static data
    public int dungeonFloorsExplored ;
    public int enemiesKilled         ;
    public int ripostes              ;
    public int blocks                ;
    public int hits                  ;
    public int fullChargeHits        ;
    public int arrowsFired           ;
    public int randomizedItemsDropped;

}

[Serializable]
public class ItemData
{
    public string title;
    public string name;
    public int tier;
    public int graphicId;

    public EquipmentType type;
    public EquipmentSlot slot;
    public Quality quality;
    public ArmorType armorType;

    public int dmgMod;
    public int armorMod;
    public int sta;
    public int str;
    public int agi;
    public int rangedProjectile;
    public int sellValue;
    public int buyValue;
}
