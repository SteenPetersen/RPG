using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootController : MonoBehaviour {

    public static LootController instance;

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
    }

    public float lootChance;

    public List<GameObject> enemyTier0 = new List<GameObject>();
    public List<GameObject> enemyTier1 = new List<GameObject>();
    public List<GameObject> enemyTier2 = new List<GameObject>();

    public List<GameObject> chestTier0 = new List<GameObject>();
    public List<GameObject> chestTier1 = new List<GameObject>();
    public List<GameObject> chestTier2 = new List<GameObject>();


    void Start () {
		
	}
	
	void Update () {
		
	}

    public void EnemyLoot(int tier, Vector2 position)
    {
        // see if anything dropped
        bool loot = false;

        int lootCheck = UnityEngine.Random.Range(0, 100);

        if (lootCheck < lootChance)
        {
            loot = true;
        }


        // spawn loot at location
        if (loot)
        {
            switch (tier)
            {
                case 0:
                    int rnd = UnityEngine.Random.Range(0, enemyTier0.Count);
                    Instantiate(enemyTier0[rnd], position, Quaternion.identity);
                    // play loot sound
                    SoundManager.instance.PlayUiSound("lootdrop");
                    // add to statistics
                    loot = false;
                    break;
            }
        }

    }

    public GameObject DetermineChestLoot( int tier )
    {
        switch (tier)
        {
            case 0:
                int rnd = UnityEngine.Random.Range(0, enemyTier0.Count);
                var go = enemyTier0[rnd];

                return go;
        }

        throw new Exception("there is no recognizable tier upon which to run the switch case ");
    }
}
