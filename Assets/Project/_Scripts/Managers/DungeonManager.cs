using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour {

    public static DungeonManager _instance;
    public static DungeonManager instance { get { return _instance; } }

    // statics
    public static int dungeonLevel;

    /// <summary>
    /// Property where we set the image of the bosskey
    /// </summary>
    public bool _PlayerHasBossKey
    {
        get
        {
            return playerHasBossKey;
        }

        set
        {
            playerHasBossKey = value;
        }
    }

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
    }

    public List<GameObject> enemiesInDungeon = new List<GameObject>();
    public GameObject bossRoomKey;  // accessed by the lootcontroller
    public GameObject chestKey;   // accessed by the lootcontroller
    public bool townPortalDropped;

    private bool playerHasBossKey;
    public bool bossKeyHasDropped;
    public bool bossRoomAvailable;

    public bool dungeonReady;

    public bool teleport;
}
