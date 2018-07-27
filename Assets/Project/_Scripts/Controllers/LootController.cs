﻿using System;
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
                    EquipmentGenerator._instance.CreateDroppable(position);
                    // play loot sound
                    SoundManager.instance.PlayUiSound("lootdrop");
                    // add to statistics
                    break;
            }
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
                var go = EquipmentGenerator._instance.CreateDroppable();
                return go;
        }

        throw new Exception("there is no recognizable tier upon which to run the switch case ");
    }
}
