using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour {

    public static DungeonManager _instance;
    public static DungeonManager Instance { get { return _instance; } }
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

    public bool playerHasBossKey;
    public bool bossKeyHasDropped;
    public bool bossRoomAvailable;

    public bool teleport;
}
