using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour {

    public GameObject[] enemies;
    int spawnTime;
    float spawnTimer;

    public int spawnDelay;

    public Text uiText;

	// Use this for initialization
	void Start () {
        spawnTimer = spawnDelay;
	}
	
	// Update is called once per frame
	void Update () {
        SpawnEnemies();
        uiText.text = "Time to next monster: " + (int)spawnTimer;
	}

    void SpawnEnemies()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer < spawnTime)
        {
            spawnTimer = spawnDelay;
            int randomMonster = Random.Range(0, enemies.Length);

            int spawnPointX = Random.Range(-10, 10);
            int spawnPointY = Random.Range(-10, 10);
            Vector3 spawnPosition = new Vector3(spawnPointX, spawnPointY, 0);

            Instantiate(enemies[randomMonster], spawnPosition, Quaternion.identity);
        }
    }
}
