using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootController : MonoBehaviour {

    public static LootController instance;

    [SerializeField] float droppedLootDistance;
    [SerializeField] float droppedLootHeight;

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
    /// Used when iterating through positions and removing them
    /// This is updated on each iteration so we dont run out of positions
    /// and get an indexoutofrange exception.
    /// </summary>
    Vector2 defaultLootPos;

    /// <summary>
    /// Needs to be exposed for the equipmentGenerator
    /// </summary>
    public float MyDroppedLootHeight
    {
        get
        {
            return droppedLootHeight;
        }

        set
        {
            droppedLootHeight = value;
        }
    }

    public float MyDroppedLootDistance
    {
        get
        {
            return droppedLootDistance;
        }

        set
        {
            droppedLootDistance = value;
        }
    }


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
                    EquipmentGenerator._instance.CreateDroppable(position, tier, true);
                    
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

    /// <summary>
    /// Finds a random position amongst 24 evenly distributed points which fill the
    /// criteria of not being inside or on the otherside of a wall or innaccessable to the player.
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    public List<Vector2> FindAllEndPositions(Transform origin)
    {
        int obstacleLayer = 13;
        int destructableLayer = 19;
        var obstacleLayerMask = 1 << obstacleLayer;
        var destructableLayerMask = 1 << destructableLayer;

        var finalMask = obstacleLayerMask | destructableLayerMask;

        //Start on ground
        Vector3 beginRayCast = new Vector3(origin.position.x, origin.position.y, 0);

        // initial direction is from obj and up
        Vector2 direction = origin.position + Vector3.up;

        List<Vector2> p = new List<Vector2>();

        for (int i = 1; i <= 24; i++)
        {
            Vector2 dir = direction.Rotate(15f * i);

            RaycastHit2D hit = Physics2D.Raycast(beginRayCast, dir, MyDroppedLootDistance, finalMask);

            if (hit.collider == null)
            {
                bool canSpawn = CheckAreaForWalls((Vector2)beginRayCast + (dir.normalized * MyDroppedLootDistance));

                if (canSpawn)
                {
                    Vector2 toAdd = (Vector2)beginRayCast + (dir.normalized * MyDroppedLootDistance);
                    p.Add(toAdd);

                    Vector2 toAddClose = (Vector2)beginRayCast + (dir.normalized * (MyDroppedLootDistance / 2));
                    p.Add(toAddClose);
                }
            }
        }

        return p;
    }

    /// <summary>
    /// OverLoad METHOD: Finds a random position amongst 24 evenly distributed points which fill the
    /// criteria of not being inside or on the otherside of a wall or innaccessable to the player.
    /// </summary>
    /// <param name="origin">startPosition</param>
    /// <param name="setDistance">a distance that can be set from the calling function</param>
    /// <returns></returns>
    public List<Vector2> FindAllEndPositions(Transform origin, float setDistance)
    {
        int obstacleLayer = 13;
        int destructableLayer = 19;
        var obstacleLayerMask = 1 << obstacleLayer;
        var destructableLayerMask = 1 << destructableLayer;

        var finalMask = obstacleLayerMask | destructableLayerMask;

        /// Start on the ground
        Vector3 beginRayCast = new Vector3(origin.position.x, origin.position.y, 0);

        /// initial direction is from the obj and up
        Vector2 direction = origin.position + Vector3.up;

        List<Vector2> p = new List<Vector2>();

        for (int i = 1; i <= 24; i++)
        {
            Vector2 dir = direction.Rotate(15f * i);

            RaycastHit2D hit = Physics2D.Raycast(beginRayCast, dir, setDistance, finalMask);


            if (hit.collider == null)
            {
                bool canSpawn = CheckAreaForWalls((Vector2)beginRayCast + (dir.normalized * setDistance));

                if (canSpawn)
                {
                    Debug.DrawRay(beginRayCast, (dir.normalized * setDistance), Color.red, 2f);

                    Vector2 toAdd = (Vector2)beginRayCast + (dir.normalized * setDistance);
                    p.Add(toAdd);

                    Vector2 toAddClose = (Vector2)beginRayCast + (dir.normalized * (setDistance / 2));
                    p.Add(toAddClose);
                }
            }
        }

        return p;
    }

    /// <summary>
    /// Checks a specific area for wall colliders
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckAreaForWalls(Vector3 pos)
    {
        int obstaclesLayer = 13;
        var obstacleLayerMask = 1 << obstaclesLayer;

        Collider2D[] wallColliders = Physics2D.OverlapCircleAll(pos, 1.2f, obstacleLayerMask);

        if (wallColliders.Length != 0)
        {
            return false;
        }

        return true;
    }

    internal Vector2 SelectEndPosition(List<Vector2> positions)
    {
        positions.Shuffle();

        if (positions.Count > 0)
        {
            defaultLootPos = positions[0];
            Vector2 tmp = positions[0];
            positions.RemoveAt(0);
            return tmp;
        }

        return defaultLootPos;
    }

    public Vector2 FindEndPosition(Transform t, float dist)
    {
        List<Vector2> positions = new List<Vector2>();

        positions = FindAllEndPositions(t.transform, dist);

        return SelectEndPosition(positions);
    }
}
