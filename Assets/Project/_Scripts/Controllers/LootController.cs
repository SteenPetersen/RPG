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


    /// <summary>
    /// Checks if the enemy has loot and if it does, runs a switch statement to randomly drop loot from a list of items corresponding to the tier provided
    /// </summary>
    /// <param name="tier">The tier variable of this enemy</param>
    /// <param name="position">The position in which to spawn this item</param>
    public void EnemyLoot(int tier, Vector2 position)
    {
        int lootCheck = UnityEngine.Random.Range(0, 100);

        // spawn loot at location
        if (lootCheck < lootChance)
        {
            switch (tier)
            {
                case 0:
                    EquipmentGenerator._instance.CreateDroppable(position, tier);
                    
                    // play loot sound
                    SoundManager.instance.PlayUiSound("lootdrop");

                    // Add to statistics
                    GameDetails.randomizedItemsDropped++;
                    break;
            }
        }

    }

    /// <summary>
    /// Guarantees that boss key can drop and increases the chance of it dropping depending
    /// on the amount of enemies still in the dungeon
    /// </summary>
    /// <param name="amountOfEnemies">Amount of enemies in the dungeon</param>
    /// <param name="pos">Position at which to instantiate the key if it drops</param>
    public void EnemyBossRoomKeyDrop(int amountOfEnemies, Vector3 pos)
    {
        float p = new float();

        if (amountOfEnemies > 0)
        {
            p = (100 / amountOfEnemies);
        }

        float n = UnityEngine.Random.Range(0, 100);

        //Debug.LogWarning("Percent chance to drop key was " + p + " and random number was " + n + " amount of enemies was " + amountOfEnemies);

        if (n <= p)
        {
            Instantiate(DungeonManager.instance.bossRoomKey, pos, Quaternion.identity);
            DungeonManager.instance.bossKeyHasDropped = true;
            //Debug.LogWarning("Boss Key dropped!");
        }
    }

    /// <summary>
    /// Runs a switch statement on the tier parameter and returns a Gameobject from a corresponding list of items
    /// </summary>
    /// <param name="tier">The tier of the loot which is to be returned.</param>
    /// <returns></returns>
    public GameObject DetermineChestLoot( int tier )
    {
        switch (tier)
        {
            case 0:
                return EquipmentGenerator._instance.CreateDroppable(tier);

            case 1:
                return EquipmentGenerator._instance.CreateDroppable(tier);

            case 2:
                return EquipmentGenerator._instance.CreateDroppable(tier);
        }

        throw new Exception("there is no recognizable tier upon which to run the switch case ");
    }
}
