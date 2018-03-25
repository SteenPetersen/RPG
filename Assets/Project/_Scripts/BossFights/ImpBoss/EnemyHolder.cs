using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHolder : MonoBehaviour {

    public static EnemyHolder instance;

    public List<GameObject> enemies = new List<GameObject>();

    public int maxAmountOfEnemies;

    public Transform[] spawnPoints;

    public GameObject[] enemy;
    public float timer;
    public float spawnDelay;

    public bool bossIsAlive = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (bossIsAlive)
        {
            // start the timer counting down
            timer -= Time.deltaTime;

            // if the timer is under 0
            if (timer < 0)
            {
                //reset the timer
                timer = spawnDelay;


                // if the amount of enemies is below the max amount of enemies
                if (enemies.Count < maxAmountOfEnemies)
                {
                    // run the spawn enemy logic
                    SpawnEnemy();
                }
            }
        }
    }


    private void SpawnEnemy()
    {
        Debug.Log("spawning enemy");
        // generate a random number corresponding to the amount of enemies in a list
        int rand = UnityEngine.Random.Range(0, enemy.Length - 1);

        Debug.Log(rand);

        // get a random number correspondning to a spawn location
        int randLoc = UnityEngine.Random.Range(0, 4);

        // generate a pos based on a spawn Position
        Vector3 pos = new Vector3(spawnPoints[randLoc].transform.position.x, spawnPoints[randLoc].transform.position.y, 0);

        // spawn an enemy at that position and make it a child of this object
        var spawnedEnemy = Instantiate(enemy[rand], pos, Quaternion.identity, this.transform);

        // add the enemy to the list of enemies
        enemies.Add(spawnedEnemy);

        // set a reference to the script on the enemy
        var script = spawnedEnemy.gameObject.GetComponent<EnemyAI>();

        // increase the aggro distance of this enemy
        script.HyperAlert();
    }
}
