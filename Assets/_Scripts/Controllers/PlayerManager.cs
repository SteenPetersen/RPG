using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class PlayerManager : MonoBehaviour {

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
        Load();
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

        bf.Serialize(file, data);
        file.Close();
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
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            File.Delete(Application.persistentDataPath + "/playerInfo.dat");
        }
    }
}

[Serializable]
class PlayerData
{
    public float currentHealth;
    public float maxHealth;
    public float experience;
    public int stage;

    public int kingSpeech;
}
