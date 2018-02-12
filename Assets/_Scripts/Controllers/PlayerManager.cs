using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;

public class PlayerManager : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void SyncFiles();

    [DllImport("__Internal")]
    private static extern void WindowAlert(string message);


    #region Singleton
    public static PlayerManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            //DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion
    #region Dialogue
    public Vector3 dialogueRight = new Vector3(1.86f, -9f, -6.3f);
    public Vector3 dialogueLeft = new Vector3(1.86f, -9f, -6.3f);

    public GameObject dialogueCamera;

    #endregion

    public int stage;
    public int kingSpeech;
    public bool paused;
    [SerializeField]
    public List<GameObject> generalObjects = new List<GameObject>();

    public Plane m_Plane = new Plane(Vector3.forward, Vector3.zero);

    public GameObject player;
    PlayerStats playerStats;

    private void Start()
    {
        playerStats = player.GetComponent<PlayerStats>();
        //Load();
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

        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        if (playerStats == null)
        {
            playerStats = player.GetComponent<PlayerStats>();
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
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

        PlayerData data = new PlayerData();
        data.currentHealth = playerStats.currentHealth;
        data.maxHealth = playerStats.maxHealth;
        data.experience = playerStats.exp;
        data.stage = stage;
        data.kingSpeech = kingSpeech;

        data.equippedItems = FetchItems("currentlyEquipped");
        data.itemsInBag = FetchItems("itemsInBag");

        bf.Serialize(file, data);
        file.Close();

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            SyncFiles();
        }
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            playerStats.currentHealth = data.currentHealth;
            playerStats.maxHealth = data.maxHealth;
            playerStats.exp = data.experience;
            stage = data.stage;
            kingSpeech = data.kingSpeech;

            loadItems(data.equippedItems, 0);
            loadItems(data.itemsInBag, 1);

        }
    }

    private static void PlatformSafeMessage(string message)
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

    List<string> FetchItems(string loc)
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
                    items.Add(string.Empty);
                }
                if (Inventory.instance.itemsInBag[i] != null)
                {
                    items.Add(Inventory.instance.itemsInBag[i].name);
                }
            }
        }

        return items;

    }

    void loadItems(List<string> items, int target)
    {
        Debug.Log("Loading");
        if (target == 0)
        {
            List<Equipment> tmp = new List<Equipment>(EquipmentManager.instance.currentEquipment.Count);
            foreach (var item in items)
            {
                if (item == string.Empty)
                {
                    tmp.Add(null);
                    Debug.Log("Adding null to tmp");

                }
                else
                {
                    var tmpEquip = Instantiate(Resources.Load("Equipment/" + item, typeof(Equipment))) as Equipment;
                    tmp.Add(tmpEquip);
                    Debug.Log("adding tmpequip to tmp" + tmpEquip.name);

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

        else if (target == 1)
        {
            foreach (string item in items)
            {
                var tmp = Resources.Load<Item>(item) as Item;
                Inventory.instance.itemsInBag.Add(tmp);
            }
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

    // Progress
    public int kingSpeech;
    public int stage;



    // Currently equipped Items
    public List<string> equippedItems;


    // Items in Bag
    public List<string> itemsInBag;
}
