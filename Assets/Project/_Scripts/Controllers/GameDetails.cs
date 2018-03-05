using System;
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
    public static GameDetails instance;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    #region Dialogue
    public Vector3 dialogueNPCIsStandingOnTheRight = new Vector3(0f, -9f, -6.3f);
    public Vector3 dialogueNPCIsStandingOnTheLeft = new Vector3(1.86f, -9f, -6.3f);

    public GameObject dialogueCamera;

    #endregion

    public Image fadeToBlack;
    public float fadeSpeed;

    public ParticleSystem playerFeedbackSystem;
    public Text playerFeedbackText;

    public ParticleSystem gameSaved;

    public int stage;
    public int kingSpeech;
    public bool paused, loadingScene;
    [SerializeField]
    public GameObject[] generalObjects;

    public Plane m_Plane = new Plane(Vector3.forward, Vector3.zero);

    public GameObject player;
    PlayerStats playerStats;

    //private void OnEnable()
    //{
    //    // subscribe to notice is a scene is loaded.
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    private void Start()
    {
        if (player == null)
        {
            player = PlayerController.instance.gameObject;
        }

        playerStats = player.GetComponent<PlayerStats>();

        ui = gameObject.transform.GetComponentsInChildren<Canvas>();

        StartCoroutine(enableUi());
    }

    void Update()
    {
        HandleInput();

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

    }

    private void DeathSequence()
    {
        fadeToBlack.enabled = true;
        fadeToBlack.color = new Color(0, 0, 0, fadeSpeed);
        fadeSpeed += 0.01f;


        if (fadeToBlack.color.a >= 1 && !loadingScene)
        {
            if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
            {
                Load();
            }
            else
            {
                PlayerController.instance.gameObject.transform.position = new Vector2(-12f, 4);
                playerStats.Heal((int)playerStats.maxHealth);
                SceneManager.LoadScene(0);
                PlayerController.instance.isDead = false;
                loadingScene = true;
            }

        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
        }
    }

    public void KillPlayer()
    {
        Debug.Log("killing player");
    }

    public void Save()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene == 1)
            return;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

        PlayerData data = new PlayerData();
        data.currentHealth = playerStats.currentHealth;
        data.maxHealth = playerStats.maxHealth;
        data.experience = playerStats.exp;
        data.stage = stage;
        data.kingSpeech = kingSpeech;

        data.equippedItems = FindItemsToSave("currentlyEquipped");
        data.itemsInBag = FindItemsToSave("itemsInBag");

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

            PlayerController.instance.speed = PlayerController.instance.normalSpeed;
            playerStats.currentHealth = 0;
            playerStats.Heal((int)data.currentHealth);
            playerStats.maxHealth = data.maxHealth;
            playerStats.exp = data.experience;
            stage = data.stage;
            kingSpeech = data.kingSpeech;

            loadItems(data.equippedItems, "currentlyEquipped");
            loadItems(data.itemsInBag, "itemsInBag");

            SceneManager.LoadScene(data.zone);
            player.transform.position = new Vector2(data.locationX, data.locationY);


        }
    }

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
        List<Item> items = new List<Item>();

        for (int i = 0; i < Inventory.instance.itemsInBag.Count; i++)
        {
            if (Inventory.instance.itemsInBag[i] != null)
            {
                items.Add(Inventory.instance.itemsInBag[i]);
                //Debug.Log("adding " + Inventory.instance.itemsInBag[i] + " to list");
            }
        }

        foreach (var item in items)
        {
            Inventory.instance.Remove(item);
        }

    }

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

    List<string> FindItemsToSave(string loc)
    {
        List<string> items = new List<string>();

        if (loc == "currentlyEquipped")
        {
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
        }
        else if (loc == "itemsInBag")
        {
            for (int i = 0; i < Inventory.instance.itemsInBag.Count; i++)
            {
                if (Inventory.instance.itemsInBag[i] == null)
                {
                    //items.Add(string.Empty);
                }
                if (Inventory.instance.itemsInBag[i] != null)
                {
                    items.Add(Inventory.instance.itemsInBag[i].name);
                }
            }
        }

        return items;

    }

    void loadItems(List<string> items, string setToLoad)
    {
        //Debug.Log("Loading");
        if (setToLoad == "currentlyEquipped")
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

        else if (setToLoad == "itemsInBag")
        {
            List<Item> tmp = new List<Item>(Inventory.instance.space);
            foreach (var item in items)
            {
                if (Resources.Load("Equipment/" + item) != null)
                {
                    var tmpItem = Instantiate(Resources.Load("Equipment/" + item, typeof(Item))) as Item;
                    tmp.Add(tmpItem);

                }
                else if (Resources.Load("Equipment/" + item) == null)
                {
                    var tmpItem = Instantiate(Resources.Load("Items/" + item, typeof(Item))) as Item;
                    tmp.Add(tmpItem);
                }

            }

            for (int i = 0; i < items.Count; i++)
            {
                if (tmp[i] != null)
                {
                    Inventory.instance.AddItemToBag(tmp[i]);
                }
            }
        }
    }

    private void OnDisable()
    {
        // unsubscribe from the scenemanager.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        if (playerStats == null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        if (PlayerController.instance.isDead)
        {
            PlayerController.instance.isDead = false;
            PlayerController.instance.anim.SetTrigger("LoadGame");
        }

        if (ui != null)
        {
            foreach (Canvas canvas in ui)
            {
                canvas.enabled = true;

            }
        }


        StartCoroutine(enableUi());

    }

    IEnumerator enableUi()
    {
        yield return new WaitForSeconds(1.5f);

        //Debug.Log(ui.Length + "enabling ui");


        StartCoroutine(UnFade());
        loadingScene = false;

    }
    IEnumerator UnFade()
    {
        AstarPath.active.Scan();

        while (fadeToBlack.color.a >= 0.02)
        {
            fadeToBlack.enabled = true;
            fadeToBlack.color = new Color(0, 0, 0, fadeSpeed);
            fadeSpeed -= 0.02f;
            yield return new WaitForSeconds(0.01f);
        }

        yield return null;
    }
}

[Serializable]
class PlayerData
{
    // Stats
    public float currentHealth;
    public float maxHealth;
    public float experience;

    // Progress
    public int kingSpeech;
    public int stage;


    // Currently equipped Items
    public List<string> equippedItems;


    // Items in Bag
    public List<string> itemsInBag;

    // zone information
    public int zone;
    public float locationX;
    public float locationY;

}
